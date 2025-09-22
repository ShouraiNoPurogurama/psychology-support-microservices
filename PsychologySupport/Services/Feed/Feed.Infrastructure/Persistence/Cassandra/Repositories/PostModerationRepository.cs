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
}
