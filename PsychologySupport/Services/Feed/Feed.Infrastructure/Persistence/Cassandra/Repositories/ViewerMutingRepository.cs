using Cassandra;
using Cassandra.Mapping;
using Feed.Application.Abstractions.ViewerMuting;
using Feed.Domain.ViewerMuting;
using Feed.Infrastructure.Persistence.Cassandra.Mappings;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Repositories;

public sealed class ViewerMutingRepository : IViewerMutingRepository
{
    private readonly ISession _session;

    public ViewerMutingRepository(ISession session)
    {
        _session = session;
    }

    public async Task<bool> AddIfNotExistsAsync(ViewerMuted entity, CancellationToken ct)
    {
        var rowData = ViewerMutingMapper.ToRow(entity);
        var cql = @"INSERT INTO viewer_muted_alias (alias_id, muted_alias_id, muted_since)
                    VALUES (?, ?, ?) IF NOT EXISTS";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(rowData.AliasId, rowData.MutedAliasId, rowData.MutedSince).SetIdempotence(true);

        RowSet rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);

        var resultRow = rs.FirstOrDefault();
        if (resultRow == null)
        {
            return false;
        }

        return resultRow.GetValue<bool>("[applied]");
    }

    public async Task<bool> RemoveAsync(Guid aliasId, Guid mutedAliasId, CancellationToken ct)
    {
        var cql = @"DELETE FROM viewer_muted_alias WHERE alias_id = ? AND muted_alias_id = ?";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(aliasId, mutedAliasId).SetIdempotence(true);
        await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid aliasId, Guid mutedAliasId, CancellationToken ct)
    {
        var cql = @"SELECT alias_id FROM viewer_muted_alias WHERE alias_id = ? AND muted_alias_id = ? LIMIT 1";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(aliasId, mutedAliasId).SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return rs.Any();
    }

    public async Task<IReadOnlyList<ViewerMuted>> GetAllByViewerAsync(Guid aliasId, CancellationToken ct)
    {
        var cql = @"SELECT alias_id, muted_alias_id, muted_since FROM viewer_muted_alias WHERE alias_id = ?";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(aliasId).SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);

        var list = new List<ViewerMuted>(128);
        foreach (var row in rs)
        {
            var r = new ViewerMutedAliasRow
            {
                AliasId = row.GetValue<Guid>("alias_id"),
                MutedAliasId = row.GetValue<Guid>("muted_alias_id"),
                MutedSince = row.GetValue<DateTimeOffset?>("muted_since")
            };
            list.Add(ViewerMutingMapper.ToDomain(r));
        }

        return list;
    }
}
