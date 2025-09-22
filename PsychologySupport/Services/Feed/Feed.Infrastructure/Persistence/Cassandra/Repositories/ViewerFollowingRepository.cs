using Cassandra;
using Cassandra.Mapping;
using Feed.Application.Abstractions.ViewerFollowing;
using Feed.Domain.ViewerFollowing;
using Feed.Infrastructure.Persistence.Cassandra.Mappings;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Repositories;

public sealed class ViewerFollowingRepository : IViewerFollowingRepository
{
    private readonly ISession _session;
    private readonly IMapper _mapper;

    public ViewerFollowingRepository(ISession session)
    {
        _session = session;
        _mapper = new Mapper(session);
    }

    public async Task<bool> AddIfNotExistsAsync(ViewerFollow entity, CancellationToken ct)
    {
        var rowData = ViewerFollowingMapper.ToRow(entity);
        var cql = @"INSERT INTO follows_by_viewer (alias_id, followed_alias_id, since)
                    VALUES (?, ?, ?) IF NOT EXISTS";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(rowData.AliasId, rowData.FollowedAliasId, rowData.Since).SetIdempotence(true);
        
        RowSet rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);

        // Get the first row from the result set
        var resultRow = rs.FirstOrDefault();

        // If the result is null, the operation did not apply
        if (resultRow == null)
        {
            return false;
        }

        // Read the boolean value from the "[applied]" column
        return resultRow.GetValue<bool>("[applied]");
    }

    public async Task<bool> RemoveAsync(Guid aliasId, Guid followedAliasId, CancellationToken ct)
    {
        var cql = @"DELETE FROM follows_by_viewer WHERE alias_id = ? AND followed_alias_id = ?";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(aliasId, followedAliasId).SetIdempotence(true);
        await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid aliasId, Guid followedAliasId, CancellationToken ct)
    {
        var cql = @"SELECT alias_id FROM follows_by_viewer WHERE alias_id = ? AND followed_alias_id = ? LIMIT 1";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(aliasId, followedAliasId).SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return rs.Any();
    }

    public async Task<IReadOnlyList<ViewerFollow>> GetAllByViewerAsync(Guid aliasId, CancellationToken ct)
    {
        var cql = @"SELECT alias_id, followed_alias_id, since FROM follows_by_viewer WHERE alias_id = ?";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(aliasId).SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);

        var list = new List<ViewerFollow>(128);
        foreach (var row in rs)
        {
            var r = new FollowsByViewerRow
            {
                AliasId = row.GetValue<Guid>("alias_id"),
                FollowedAliasId = row.GetValue<Guid>("followed_alias_id"),
                Since = row.GetValue<DateTimeOffset?>("since")
            };
            list.Add(ViewerFollowingMapper.ToDomain(r));
        }

        return list;
    }
}