using Cassandra;
using Cassandra.Mapping;
using Feed.Application.Abstractions.UserFeed;
using Feed.Domain.UserFeed;
using Feed.Infrastructure.Persistence.Cassandra.Mappings;
using Feed.Infrastructure.Persistence.Cassandra.Models;
using Feed.Infrastructure.Persistence.Cassandra.Utils;

namespace Feed.Infrastructure.Persistence.Cassandra.Repositories;

public sealed class UserFeedRepository : IUserFeedRepository
{
    private readonly ISession _session;

    public UserFeedRepository(ISession session)
    {
        _session = session;
    }

    public async Task<bool> AddFeedItemAsync(UserFeedItem feedItem, CancellationToken ct)
    {
        var row = UserFeedMapper.ToRow(feedItem);
        var cql = @"INSERT INTO user_feed_by_bucket (alias_id, ymd_bucket, shard, rank_bucket, rank_i64, ts_uuid, post_id, created_at) 
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(
            row.AliasId, 
            row.YmdBucket, 
            row.Shard, 
            row.RankBucket, 
            row.RankI64, 
            row.TsUuid, 
            row.PostId, 
            row.CreatedAt).SetIdempotence(true);
        await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> RemoveFeedItemAsync(Guid aliasId, DateOnly ymdBucket, short shard, Guid postId, CancellationToken ct)
    {
        var cassandraYmdBucket = CassandraTypeMapper.ToLocalDate(ymdBucket);
        
        // Need to find the clustering key values first since they're required for deletion
        var findCql = @"SELECT rank_bucket, rank_i64, ts_uuid FROM user_feed_by_bucket 
                        WHERE alias_id = ? AND ymd_bucket = ? AND shard = ? AND post_id = ? ALLOW FILTERING";
        var findPs = await _session.PrepareAsync(findCql).ConfigureAwait(false);
        var findStmt = findPs.Bind(aliasId, cassandraYmdBucket, shard, postId).SetIdempotence(true);
        var findRs = await _session.ExecuteAsync(findStmt).ConfigureAwait(false);

        var itemRow = findRs.FirstOrDefault();
        if (itemRow == null) return false;

        var rankBucket = itemRow.GetValue<sbyte>("rank_bucket");
        var rankI64 = itemRow.GetValue<long>("rank_i64");
        var tsUuid = itemRow.GetValue<TimeUuid>("ts_uuid");

        var deleteCql = @"DELETE FROM user_feed_by_bucket 
                         WHERE alias_id = ? AND ymd_bucket = ? AND shard = ? 
                         AND rank_bucket = ? AND rank_i64 = ? AND ts_uuid = ? AND post_id = ?";
        var deletePs = await _session.PrepareAsync(deleteCql).ConfigureAwait(false);
        var deleteStmt = deletePs.Bind(aliasId, cassandraYmdBucket, shard, rankBucket, rankI64, tsUuid, postId).SetIdempotence(true);
        await _session.ExecuteAsync(deleteStmt).ConfigureAwait(false);
        return true;
    }

    public async Task<IReadOnlyList<UserFeedItem>> GetFeedItemsAsync(Guid aliasId, DateOnly ymdBucket, short shard, int limit = 50, CancellationToken ct = default)
    {
        var cassandraYmdBucket = CassandraTypeMapper.ToLocalDate(ymdBucket);
        
        var cql = @"SELECT alias_id, ymd_bucket, shard, rank_bucket, rank_i64, ts_uuid, post_id, created_at 
                    FROM user_feed_by_bucket 
                    WHERE alias_id = ? AND ymd_bucket = ? AND shard = ? 
                    ORDER BY rank_bucket DESC, rank_i64 DESC, ts_uuid DESC 
                    LIMIT ?";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(aliasId, cassandraYmdBucket, shard, limit).SetIdempotence(true);
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
        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        // Query each day and shard combination separately due to Cassandra's partitioning
        for (int day = 0; day < days && allItems.Count < limit; day++)
        {
            var queryDate = currentDate.AddDays(-day);
            
            // Query multiple shards (assuming shards 0-3 for load distribution)
            for (short shard = 0; shard <= 3 && allItems.Count < limit; shard++)
            {
                var remainingLimit = limit - allItems.Count;
                var dayShardItems = await GetFeedItemsAsync(aliasId, queryDate, shard, remainingLimit, ct);
                allItems.AddRange(dayShardItems);
            }
        }

        // Sort by timestamp and take only the requested limit
        return allItems
            .OrderByDescending(item => item.TsUuid)
            .Take(limit)
            .ToList();
    }
}
