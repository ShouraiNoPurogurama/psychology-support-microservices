using Cassandra;
using Feed.Application.Abstractions.PostRepository;
using Feed.Domain.PostReplica;
using Feed.Infrastructure.Persistence.Cassandra.Mappings;
using Feed.Infrastructure.Persistence.Cassandra.Utils;
using Microsoft.Extensions.Logging;

namespace Feed.Infrastructure.Persistence.Cassandra.Repositories;

/// <summary>
/// Cassandra repository for post replica operations.
/// Implements multi-table writes and optimized reads following the three-table pattern.
/// </summary>
public sealed class PostReplicaRepository : IPostReplicaRepository
{
    private readonly ISession _session;
    private readonly IPreparedStatementRegistry _statements;
    private readonly ILogger<PostReplicaRepository> _logger;

    // Prepared statement keys
    private const string INSERT_POSTS_REPLICA = nameof(INSERT_POSTS_REPLICA);
    private const string INSERT_PUBLIC_FINALIZED = nameof(INSERT_PUBLIC_FINALIZED);
    private const string INSERT_REPLICA_BY_ID = nameof(INSERT_REPLICA_BY_ID);
    private const string SELECT_REPLICA_BY_ID = nameof(SELECT_REPLICA_BY_ID);
    private const string DELETE_POSTS_REPLICA = nameof(DELETE_POSTS_REPLICA);
    private const string DELETE_PUBLIC_FINALIZED = nameof(DELETE_PUBLIC_FINALIZED);
    private const string DELETE_REPLICA_BY_ID = nameof(DELETE_REPLICA_BY_ID);
    private const string SELECT_PUBLIC_FINALIZED_BY_DAY = nameof(SELECT_PUBLIC_FINALIZED_BY_DAY);

    // CQL statements
    private const string INSERT_POSTS_REPLICA_CQL = @"
        INSERT INTO posts_replica (ymd_bucket, created_at, post_id, author_alias_id, visibility, status)
        VALUES (?, ?, ?, ?, ?, ?)";

    private const string INSERT_PUBLIC_FINALIZED_CQL = @"
        INSERT INTO posts_public_finalized_by_day (ymd_bucket, created_at, post_id, author_alias_id)
        VALUES (?, ?, ?, ?)";

    private const string INSERT_REPLICA_BY_ID_CQL = @"
        INSERT INTO post_replica_by_id (post_id, ymd_bucket, created_at)
        VALUES (?, ?, ?)";

    private const string SELECT_REPLICA_BY_ID_CQL = @"
        SELECT ymd_bucket, created_at FROM post_replica_by_id WHERE post_id = ?";

    private const string DELETE_POSTS_REPLICA_CQL = @"
        DELETE FROM posts_replica WHERE ymd_bucket = ? AND created_at = ? AND post_id = ?";

    private const string DELETE_PUBLIC_FINALIZED_CQL = @"
        DELETE FROM posts_public_finalized_by_day WHERE ymd_bucket = ? AND created_at = ? AND post_id = ?";

    private const string DELETE_REPLICA_BY_ID_CQL = @"
        DELETE FROM post_replica_by_id WHERE post_id = ?";

    private const string SELECT_PUBLIC_FINALIZED_BY_DAY_CQL = @"
        SELECT ymd_bucket, created_at, post_id, author_alias_id 
        FROM posts_public_finalized_by_day 
        WHERE ymd_bucket = ? 
        ORDER BY created_at DESC 
        LIMIT ?";

    public PostReplicaRepository(
        ISession session,
        IPreparedStatementRegistry statements,
        ILogger<PostReplicaRepository> logger)
    {
        _session = session;
        _statements = statements;
        _logger = logger;
    }

    public async Task<bool> AddPublicFinalizedPostAsync(
        Guid postId,
        Guid authorAliasId,
        string visibility,
        string status,
        DateOnly? ymdBucket = null,
        DateTimeOffset? createdAt = null,
        CancellationToken ct = default)
    {
        try
        {
            // Create domain objects
            var postReplica = PostReplica.Create(postId, authorAliasId, visibility, status, ymdBucket, createdAt);
            var publicFinalized = PostPublicFinalizedByDay.Create(postId, authorAliasId, ymdBucket, createdAt);
            var replicaById = PostReplicaById.Create(postId, ymdBucket, createdAt);

            // Convert to rows
            var replicaRow = PostReplicaMapper.ToRow(postReplica);
            var publicRow = PostReplicaMapper.ToRow(publicFinalized);
            var byIdRow = PostReplicaMapper.ToRow(replicaById);

            // Prepare statements
            var psReplica = await _statements.GetStatementAsync(INSERT_POSTS_REPLICA, INSERT_POSTS_REPLICA_CQL);
            var psPublic = await _statements.GetStatementAsync(INSERT_PUBLIC_FINALIZED, INSERT_PUBLIC_FINALIZED_CQL);
            var psById = await _statements.GetStatementAsync(INSERT_REPLICA_BY_ID, INSERT_REPLICA_BY_ID_CQL);

            // Create batch statement for atomic writes
            var batch = new BatchStatement();

            // Add statements to batch
            batch.Add(psReplica.Bind(
                replicaRow.YmdBucket,
                replicaRow.CreatedAt,
                replicaRow.PostId,
                replicaRow.AuthorAliasId,
                replicaRow.Visibility,
                replicaRow.Status));

            batch.Add(psPublic.Bind(
                publicRow.YmdBucket,
                publicRow.CreatedAt,
                publicRow.PostId,
                publicRow.AuthorAliasId));

            batch.Add(psById.Bind(
                byIdRow.PostId,
                byIdRow.YmdBucket,
                byIdRow.CreatedAt));

            // Set batch properties
            batch.SetConsistencyLevel(ConsistencyLevel.Quorum);
            batch.SetIdempotence(true);

            // Execute batch
            await _session.ExecuteAsync(batch);

            _logger.LogDebug("Added public finalized post {PostId} to all replica tables", postId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding public finalized post {PostId} to replica tables", postId);
            throw;
        }
    }

    public async Task<bool> DeletePostReplicaAsync(Guid postId, CancellationToken ct = default)
    {
        try
        {
            // First, lookup the partition key from post_replica_by_id
            var lookup = await GetPostLookupAsync(postId, ct);
            if (!lookup.HasValue)
            {
                _logger.LogWarning("Post {PostId} not found in lookup table for deletion", postId);
                return false;
            }

            var (ymdBucket, createdAt) = lookup.Value;
            var cassandraYmdBucket = CassandraTypeMapper.ToLocalDate(ymdBucket);
            var cassandraCreatedAt = CassandraTypeMapper.ToTimeUuid(createdAt);

            // Prepare delete statements
            var psDeleteReplica = await _statements.GetStatementAsync(DELETE_POSTS_REPLICA, DELETE_POSTS_REPLICA_CQL);
            var psDeletePublic = await _statements.GetStatementAsync(DELETE_PUBLIC_FINALIZED, DELETE_PUBLIC_FINALIZED_CQL);
            var psDeleteById = await _statements.GetStatementAsync(DELETE_REPLICA_BY_ID, DELETE_REPLICA_BY_ID_CQL);

            // Create batch for atomic deletes
            var batch = new BatchStatement();

            batch.Add(psDeleteReplica.Bind(cassandraYmdBucket, cassandraCreatedAt, postId));
            batch.Add(psDeletePublic.Bind(cassandraYmdBucket, cassandraCreatedAt, postId));
            batch.Add(psDeleteById.Bind(postId));

            batch.SetConsistencyLevel(ConsistencyLevel.Quorum);
            batch.SetIdempotence(true);

            // Execute batch
            await _session.ExecuteAsync(batch);

            _logger.LogDebug("Deleted post {PostId} from all replica tables", postId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post {PostId} from replica tables", postId);
            throw;
        }
    }

    public async Task<(DateOnly YmdBucket, Guid CreatedAt)?> GetPostLookupAsync(Guid postId, CancellationToken ct = default)
    {
        try
        {
            var ps = await _statements.GetStatementAsync(SELECT_REPLICA_BY_ID, SELECT_REPLICA_BY_ID_CQL);
            var stmt = ps.Bind(postId)
                .SetConsistencyLevel(ConsistencyLevel.LocalOne)
                .SetIdempotence(true);

            var rs = await _session.ExecuteAsync(stmt);
            var row = rs.FirstOrDefault();

            if (row == null)
                return null;

            var ymdBucket = CassandraTypeMapper.ToDateOnly(row.GetValue<LocalDate>("ymd_bucket"));
            var createdAt = CassandraTypeMapper.ToGuid(row.GetValue<TimeUuid>("created_at"));

            return (ymdBucket, createdAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error looking up post {PostId} in replica by id table", postId);
            throw;
        }
    }

    public async Task<IReadOnlyList<Guid>> GetMostRecentPublicPostsAsync(
        int days = 7,
        int limit = 500,
        CancellationToken ct = default)
    {
        try
        {
            var allPostIds = new List<Guid>();
            var currentDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
            var ps = await _statements.GetStatementAsync(SELECT_PUBLIC_FINALIZED_BY_DAY, SELECT_PUBLIC_FINALIZED_BY_DAY_CQL);

            // Query each day, starting from today going backwards
            for (int dayOffset = 0; dayOffset < days && allPostIds.Count < limit; dayOffset++)
            {
                var queryDate = currentDate.AddDays(-dayOffset);
                var cassandraDate = CassandraTypeMapper.ToLocalDate(queryDate);
                var remainingLimit = limit - allPostIds.Count;

                var stmt = ps.Bind(cassandraDate, remainingLimit)
                    .SetConsistencyLevel(ConsistencyLevel.LocalOne)
                    .SetIdempotence(true);

                var rs = await _session.ExecuteAsync(stmt);

                foreach (var row in rs)
                {
                    var postId = row.GetValue<Guid>("post_id");
                    allPostIds.Add(postId);

                    if (allPostIds.Count >= limit)
                        break;
                }
            }

            _logger.LogDebug("Retrieved {Count} recent public posts from {Days} days", allPostIds.Count, days);
            return allPostIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving most recent public posts");
            throw;
        }
    }
}
