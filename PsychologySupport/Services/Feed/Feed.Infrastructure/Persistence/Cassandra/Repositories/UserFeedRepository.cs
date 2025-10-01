using Cassandra;
using Cassandra.Mapping;
using Feed.Application.Abstractions.UserFeed;
using Feed.Application.Configuration;
using Feed.Domain.UserFeed;
using Feed.Infrastructure.Persistence.Cassandra.Mappings;
using Feed.Infrastructure.Persistence.Cassandra.Models;
using Feed.Infrastructure.Persistence.Cassandra.Utils;
using Microsoft.Extensions.Options;

namespace Feed.Infrastructure.Persistence.Cassandra.Repositories;

public sealed class UserFeedRepository : IUserFeedRepository
{
    private readonly ISession _session;
    private readonly IPreparedStatementRegistry _statements;
    private readonly FeedConfiguration _feedConfig;
    
    // Prepared statement keys
    private const string INSERT_FEED_ITEM = nameof(INSERT_FEED_ITEM);
    private const string SELECT_FEED_ITEMS = nameof(SELECT_FEED_ITEMS);
    private const string DELETE_FEED_ITEM = nameof(DELETE_FEED_ITEM);
    private const string SELECT_FOR_DELETE = nameof(SELECT_FOR_DELETE);

    // CQL statements
    private const string INSERT_CQL = @"INSERT INTO user_feed_by_bucket (alias_id, ymd_bucket, shard, rank_bucket, rank_i64, ts_uuid, post_id, created_at) 
                                       VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
    private const string SELECT_CQL = @"SELECT alias_id, ymd_bucket, shard, rank_bucket, rank_i64, ts_uuid, post_id, created_at 
                                       FROM user_feed_by_bucket 
                                       WHERE alias_id = ? AND ymd_bucket = ? AND shard = ? 
                                       ORDER BY rank_bucket DESC, rank_i64 DESC, ts_uuid DESC 
                                       LIMIT ?";
    private const string SELECT_FOR_DELETE_CQL = @"SELECT rank_bucket, rank_i64, ts_uuid FROM user_feed_by_bucket 
                                                   WHERE alias_id = ? AND ymd_bucket = ? AND shard = ? AND post_id = ? ALLOW FILTERING";
    private const string DELETE_CQL = @"DELETE FROM user_feed_by_bucket 
                                       WHERE alias_id = ? AND ymd_bucket = ? AND shard = ? 
                                       AND rank_bucket = ? AND rank_i64 = ? AND ts_uuid = ? AND post_id = ?";

    public UserFeedRepository(ISession session, IPreparedStatementRegistry statements, IOptions<FeedConfiguration> feedConfig)
    {
        _session = session;
        _statements = statements;
        _feedConfig = feedConfig.Value;
    }

    public async Task<bool> AddFeedItemAsync(UserFeedItem feedItem, CancellationToken ct)
    {
        var row = UserFeedMapper.ToRow(feedItem);
        var ps = await _statements.GetStatementAsync(INSERT_FEED_ITEM, INSERT_CQL);
        var stmt = ps.Bind(
            row.AliasId, 
            row.YmdBucket, 
            row.Shard, 
            row.RankBucket, 
            row.RankI64, 
            row.TsUuid, 
            row.PostId, 
            row.CreatedAt)
            .SetConsistencyLevel(ConsistencyLevel.Quorum)
            .SetIdempotence(true);
            
        await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> RemoveFeedItemAsync(Guid aliasId, DateOnly ymdBucket, short shard, Guid postId, CancellationToken ct)
    {
        var cassandraYmdBucket = CassandraTypeMapper.ToLocalDate(ymdBucket);
        
        // Find the clustering key values first
        var findPs = await _statements.GetStatementAsync(SELECT_FOR_DELETE, SELECT_FOR_DELETE_CQL);
        var findStmt = findPs.Bind(aliasId, cassandraYmdBucket, shard, postId)
            .SetConsistencyLevel(ConsistencyLevel.LocalOne)
            .SetIdempotence(true);
        var findRs = await _session.ExecuteAsync(findStmt).ConfigureAwait(false);

        var itemRow = findRs.FirstOrDefault();
        if (itemRow == null) return false;

        var rankBucket = itemRow.GetValue<sbyte>("rank_bucket");
        var rankI64 = itemRow.GetValue<long>("rank_i64");
        var tsUuid = itemRow.GetValue<TimeUuid>("ts_uuid");

        var deletePs = await _statements.GetStatementAsync(DELETE_FEED_ITEM, DELETE_CQL);
        var deleteStmt = deletePs.Bind(aliasId, cassandraYmdBucket, shard, rankBucket, rankI64, tsUuid, postId)
            .SetConsistencyLevel(ConsistencyLevel.Quorum)
            .SetIdempotence(true);
        await _session.ExecuteAsync(deleteStmt).ConfigureAwait(false);
        return true;
    }

    public async Task<IReadOnlyList<UserFeedItem>> GetFeedItemsAsync(Guid aliasId, DateOnly ymdBucket, short shard, int limit = 50, CancellationToken ct = default)
    {
        var cassandraYmdBucket = CassandraTypeMapper.ToLocalDate(ymdBucket);
        
        var ps = await _statements.GetStatementAsync(SELECT_FEED_ITEMS, SELECT_CQL);
        var stmt = ps.Bind(aliasId, cassandraYmdBucket, shard, limit)
            .SetConsistencyLevel(ConsistencyLevel.LocalOne)
            .SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);

        var list = new List<UserFeedItem>();
        foreach (var row in rs)
        {
            var feedRow = new UserFeedByBucketRow
            {
                AliasId = row.GetValue<Guid>("alias_id"),
                YmdBucket = row.GetValue<LocalDate>("ymd_bucket"),
                Shard = row.GetValue<short>("shard"),
                RankBucket = row.GetValue<sbyte>("rank_bucket"),
                RankI64 = row.GetValue<long>("rank_i64"),
                TsUuid = row.GetValue<TimeUuid>("ts_uuid"),
                PostId = row.GetValue<Guid>("post_id"),
                CreatedAt = row.GetValue<DateTimeOffset?>("created_at")
            };
            list.Add(UserFeedMapper.ToDomain(feedRow));
        }

        return list;
    }

    public async Task<IReadOnlyList<UserFeedItem>> GetUserFeedAsync(Guid aliasId, int days = 7, int limit = 100, CancellationToken ct = default)
    {
        var allItems = new List<UserFeedItem>();
        var currentDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        var shardCount = _feedConfig.FeedShardCount;

        // Query each day with parallel shard queries for better performance
        for (int day = 0; day < days && allItems.Count < limit; day++)
        {
            var queryDate = currentDate.AddDays(-day);
            var remainingLimit = limit - allItems.Count;
            
            // Parallel shard queries
            var shardTasks = Enumerable.Range(0, shardCount)
                .Select(shard => GetFeedItemsAsync(aliasId, queryDate, (short)shard, remainingLimit / shardCount + 1, ct))
                .ToArray();
                
            var shardResults = await Task.WhenAll(shardTasks);
            var dayItems = shardResults.SelectMany(items => items).ToList();
            
            allItems.AddRange(dayItems.Take(remainingLimit));
        }

        // Sort by timestamp and take only the requested limit
        return allItems
            .OrderByDescending(item => item.TsUuid)
            .Take(limit)
            .ToList();
    }
}
