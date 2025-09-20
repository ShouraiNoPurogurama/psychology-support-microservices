using Cassandra;
using Cassandra.Mapping;
using Feed.Application.Abstractions.ViewerBlocking;
using Feed.Domain.ViewerBlocking;
using Feed.Infrastructure.Persistence.Cassandra.Mappings;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Repositories;

public sealed class ViewerBlockingRepository : IViewerBlockingRepository
{
    private readonly ISession _session;

    public ViewerBlockingRepository(ISession session)
    {
        _session = session;
    }

    public async Task<bool> AddIfNotExistsAsync(ViewerBlocked entity, CancellationToken ct)
    {
        var rowData = ViewerBlockingMapper.ToRow(entity);
        var cql = @"INSERT INTO viewer_blocked_alias (alias_id, blocked_alias_id, blocked_since)
                    VALUES (?, ?, ?) IF NOT EXISTS";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(rowData.AliasId, rowData.BlockedAliasId, rowData.BlockedSince).SetIdempotence(true);

        RowSet rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);

        var resultRow = rs.FirstOrDefault();
        if (resultRow == null)
        {
            return false;
        }

        return resultRow.GetValue<bool>("[applied]");
    }

    public async Task<bool> RemoveAsync(Guid aliasId, Guid blockedAliasId, CancellationToken ct)
    {
        var cql = @"DELETE FROM viewer_blocked_alias WHERE alias_id = ? AND blocked_alias_id = ?";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(aliasId, blockedAliasId).SetIdempotence(true);
        await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid aliasId, Guid blockedAliasId, CancellationToken ct)
    {
        var cql = @"SELECT alias_id FROM viewer_blocked_alias WHERE alias_id = ? AND blocked_alias_id = ? LIMIT 1";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(aliasId, blockedAliasId).SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return rs.Any();
    }

    public async Task<IReadOnlyList<ViewerBlocked>> GetAllByViewerAsync(Guid aliasId, CancellationToken ct)
    {
        var cql = @"SELECT alias_id, blocked_alias_id, blocked_since FROM viewer_blocked_alias WHERE alias_id = ?";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(aliasId).SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);

        var list = new List<ViewerBlocked>(128);
        foreach (var row in rs)
        {
            var r = new ViewerBlockedAliasRow
            {
                AliasId = row.GetValue<Guid>("alias_id"),
                BlockedAliasId = row.GetValue<Guid>("blocked_alias_id"),
                BlockedSince = row.GetValue<DateTimeOffset?>("blocked_since")
            };
            list.Add(ViewerBlockingMapper.ToDomain(r));
        }

        return list;
    }
}
