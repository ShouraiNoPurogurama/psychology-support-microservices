using Cassandra;
using Cassandra.Mapping;
using Feed.Application.Abstractions.FollowerTracking;
using Feed.Domain.FollowerTracking;
using Feed.Infrastructure.Persistence.Cassandra.Mappings;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Repositories;

public sealed class FollowerTrackingRepository : IFollowerTrackingRepository
{
    private readonly ISession _session;

    public FollowerTrackingRepository(ISession session)
    {
        _session = session;
    }

    public async Task<bool> AddIfNotExistsAsync(Follower entity, CancellationToken ct)
    {
        var rowData = FollowerTrackingMapper.ToRow(entity);
        var cql = @"INSERT INTO followers_by_alias (author_alias_id, alias_id, since)
                    VALUES (?, ?, ?) IF NOT EXISTS";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(rowData.AuthorAliasId, rowData.AliasId, rowData.Since).SetIdempotence(true);

        RowSet rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);

        var resultRow = rs.FirstOrDefault();
        if (resultRow == null)
        {
            return false;
        }

        return resultRow.GetValue<bool>("[applied]");
    }

    public async Task<bool> RemoveAsync(Guid authorAliasId, Guid aliasId, CancellationToken ct)
    {
        var cql = @"DELETE FROM followers_by_alias WHERE author_alias_id = ? AND alias_id = ?";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(authorAliasId, aliasId).SetIdempotence(true);
        await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid authorAliasId, Guid aliasId, CancellationToken ct)
    {
        var cql = @"SELECT author_alias_id FROM followers_by_alias WHERE author_alias_id = ? AND alias_id = ? LIMIT 1";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(authorAliasId, aliasId).SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return rs.Any();
    }

    public async Task<IReadOnlyList<Follower>> GetAllFollowersAsync(Guid authorAliasId, CancellationToken ct)
    {
        var cql = @"SELECT author_alias_id, alias_id, since FROM followers_by_alias WHERE author_alias_id = ?";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(authorAliasId).SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);

        var list = new List<Follower>(128);
        foreach (var row in rs)
        {
            var r = new FollowersByAliasRow
            {
                AuthorAliasId = row.GetValue<Guid>("author_alias_id"),
                AliasId = row.GetValue<Guid>("alias_id"),
                Since = row.GetValue<DateTimeOffset?>("since")
            };
            list.Add(FollowerTrackingMapper.ToDomain(r));
        }

        return list;
    }
}
