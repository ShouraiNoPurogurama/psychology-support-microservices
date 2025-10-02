using Cassandra;
using Cassandra.Mapping;
using Feed.Application.Abstractions.PostModeration;
using Feed.Domain.PostModeration;
using Feed.Infrastructure.Persistence.Cassandra.Mappings;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Repositories;

public sealed class PostModerationRepository : IPostModerationRepository
{
    private readonly ISession _session;

    public PostModerationRepository(ISession session)
    {
        _session = session;
    }

    public async Task<bool> SuppressAsync(PostSuppression suppression, CancellationToken ct)
    {
        var row = PostModerationMapper.ToRow(suppression);
        var cql = @"INSERT INTO post_suppressed (post_id, reason, suppressed_at, suppressed_until) 
                    VALUES (?, ?, ?, ?)";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(row.PostId, row.Reason, row.SuppressedAt, row.SuppressedUntil).SetIdempotence(true);
        await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> UnsuppressAsync(Guid postId, CancellationToken ct)
    {
        var cql = @"DELETE FROM post_suppressed WHERE post_id = ?";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(postId).SetIdempotence(true);
        await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return true;
    }

    public async Task<PostSuppression?> GetSuppressionAsync(Guid postId, CancellationToken ct)
    {
        var cql = @"SELECT post_id, reason, suppressed_at, suppressed_until FROM post_suppressed WHERE post_id = ?";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(postId).SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);

        var row = rs.FirstOrDefault();
        if (row == null) return null;

        var suppressionRow = new PostSuppressedRow
        {
            PostId = row.GetValue<Guid>("post_id"),
            Reason = row.GetValue<string?>("reason"),
            SuppressedAt = row.GetValue<DateTimeOffset?>("suppressed_at"),
            SuppressedUntil = row.GetValue<DateTimeOffset?>("suppressed_until")
        };

        return PostModerationMapper.ToDomain(suppressionRow);
    }

    public async Task<bool> IsCurrentlySuppressedAsync(Guid postId, CancellationToken ct)
    {
        var suppression = await GetSuppressionAsync(postId, ct);
        return suppression?.IsCurrentlySuppressed == true;
    }

    public async Task<IReadOnlyDictionary<Guid, PostSuppression?>> GetSuppressionsBatchAsync(
        IReadOnlyList<Guid> postIds, 
        CancellationToken ct)
    {
        if (postIds == null || postIds.Count == 0)
            return new Dictionary<Guid, PostSuppression?>();

        // Use IN query for batch retrieval
        var cql = @"SELECT post_id, reason, suppressed_at, suppressed_until 
                    FROM post_suppressed 
                    WHERE post_id IN ?";
        
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(postIds.ToList())
            .SetConsistencyLevel(ConsistencyLevel.LocalOne)
            .SetIdempotence(true);
        
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        
        var result = new Dictionary<Guid, PostSuppression?>();
        
        // Initialize all post IDs with null (not suppressed)
        foreach (var postId in postIds)
        {
            result[postId] = null;
        }
        
        // Update with actual suppression data
        foreach (var row in rs)
        {
            var suppressionRow = new PostSuppressedRow
            {
                PostId = row.GetValue<Guid>("post_id"),
                Reason = row.GetValue<string?>("reason"),
                SuppressedAt = row.GetValue<DateTimeOffset?>("suppressed_at"),
                SuppressedUntil = row.GetValue<DateTimeOffset?>("suppressed_until")
            };
            
            result[suppressionRow.PostId] = PostModerationMapper.ToDomain(suppressionRow);
        }
        
        return result;
    }

    public async Task<IReadOnlySet<Guid>> GetSuppressedPostIdsBatchAsync(
        IReadOnlyList<Guid> postIds, 
        CancellationToken ct)
    {
        if (postIds == null || postIds.Count == 0)
            return new HashSet<Guid>();

        var cql = @"SELECT post_id, suppressed_until 
                    FROM post_suppressed 
                    WHERE post_id IN ?";
        
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(postIds.ToList())
            .SetConsistencyLevel(ConsistencyLevel.LocalOne)
            .SetIdempotence(true);
        
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        
        var suppressedIds = new HashSet<Guid>();
        var now = DateTimeOffset.UtcNow;
        
        foreach (var row in rs)
        {
            var postId = row.GetValue<Guid>("post_id");
            var suppressedUntil = row.GetValue<DateTimeOffset?>("suppressed_until");
            
            // Check if currently suppressed (permanent or not yet expired)
            if (suppressedUntil == null || suppressedUntil > now)
            {
                suppressedIds.Add(postId);
            }
        }
        
        return suppressedIds;
    }
}
