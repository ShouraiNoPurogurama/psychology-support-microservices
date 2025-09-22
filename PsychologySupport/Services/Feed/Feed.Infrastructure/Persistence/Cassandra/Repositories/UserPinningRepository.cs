using Cassandra;
using Cassandra.Mapping;
using Feed.Application.Abstractions.UserPinning;
using Feed.Domain.UserPinning;
using Feed.Infrastructure.Persistence.Cassandra.Mappings;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Repositories;

public sealed class UserPinningRepository : IUserPinningRepository
{
    private readonly ISession _session;

    public UserPinningRepository(ISession session)
    {
        _session = session;
    }

    public async Task<bool> PinPostAsync(UserPinnedPost pinnedPost, CancellationToken ct)
    {
        var row = UserPinningMapper.ToRow(pinnedPost);
        var cql = @"INSERT INTO user_pinned_posts (alias_id, pinned_at, post_id) VALUES (?, ?, ?)";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(row.AliasId, row.PinnedAt, row.PostId).SetIdempotence(true);
        await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> UnpinPostAsync(Guid aliasId, Guid postId, CancellationToken ct)
    {
        // Need to find the pinned_at value first since it's part of the clustering key
        var findCql = @"SELECT pinned_at FROM user_pinned_posts WHERE alias_id = ? AND post_id = ? ALLOW FILTERING";
        var findPs = await _session.PrepareAsync(findCql).ConfigureAwait(false);
        var findStmt = findPs.Bind(aliasId, postId).SetIdempotence(true);
        var findRs = await _session.ExecuteAsync(findStmt).ConfigureAwait(false);

        var pinnedAtRow = findRs.FirstOrDefault();
        if (pinnedAtRow == null) return false;

        var pinnedAt = pinnedAtRow.GetValue<TimeUuid>("pinned_at");

        var deleteCql = @"DELETE FROM user_pinned_posts WHERE alias_id = ? AND pinned_at = ? AND post_id = ?";
        var deletePs = await _session.PrepareAsync(deleteCql).ConfigureAwait(false);
        var deleteStmt = deletePs.Bind(aliasId, pinnedAt, postId).SetIdempotence(true);
        await _session.ExecuteAsync(deleteStmt).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> IsPostPinnedAsync(Guid aliasId, Guid postId, CancellationToken ct)
    {
        var cql = @"SELECT alias_id FROM user_pinned_posts WHERE alias_id = ? AND post_id = ? LIMIT 1 ALLOW FILTERING";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(aliasId, postId).SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return rs.Any();
    }

    public async Task<IReadOnlyList<UserPinnedPost>> GetPinnedPostsAsync(Guid aliasId, CancellationToken ct)
    {
        var cql = @"SELECT alias_id, pinned_at, post_id FROM user_pinned_posts WHERE alias_id = ? ORDER BY pinned_at DESC";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(aliasId).SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);

        var list = new List<UserPinnedPost>();
        foreach (var row in rs)
        {
            var pinnedPostRow = new UserPinnedPostsRow
            {
                AliasId = row.GetValue<Guid>("alias_id"),
                PinnedAt = row.GetValue<TimeUuid>("pinned_at"),
                PostId = row.GetValue<Guid>("post_id")
            };
            list.Add(UserPinningMapper.ToDomain(pinnedPostRow));
        }

        return list;
    }
}
