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

    // This method is CORRECT because it provides both parts of the primary key.
    public async Task<bool> AddIfNotExistsAsync(Follower entity, CancellationToken ct)
    {
        var rowData = FollowerTrackingMapper.ToRow(entity);
        // NOTE: The C# code uses 'followers_of_alias' but the INSERT statement uses 'followers_of_alias'. This is a typo in the original code.
        // Assuming the table name is `followers_of_alias` as per schema.
        // The column order in the INSERT statement is different from the schema, but this is valid as long as the VALUES match.
        // Let's make it match for clarity.
        var cql = @"INSERT INTO followers_of_alias (alias_id, follower_alias_id, since)
                    VALUES (?, ?, ?) IF NOT EXISTS";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(rowData.AliasId, rowData.FollowerAliasId, rowData.Since).SetIdempotence(true);

        RowSet rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);

        var resultRow = rs.FirstOrDefault();
        return resultRow != null && resultRow.GetValue<bool>("[applied]");
    }

    // This method is CORRECT because the WHERE clause provides the full primary key.
    public async Task<bool> RemoveAsync(Guid aliasId, Guid followerAliasId, CancellationToken ct)
    {
        // Swapped parameter order to match schema PRIMARY KEY (alias_id, follower_alias_id)
        var cql = @"DELETE FROM followers_of_alias WHERE alias_id = ? AND follower_alias_id = ?";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(aliasId, followerAliasId).SetIdempotence(true);
        await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return true;
    }
    
    // This method is CORRECT because the WHERE clause provides the full primary key.
    public async Task<bool> ExistsAsync(Guid aliasId, Guid followerAliasId, CancellationToken ct)
    {
        // Swapped parameter order to match schema PRIMARY KEY (alias_id, follower_alias_id)
        var cql = @"SELECT alias_id FROM followers_of_alias WHERE alias_id = ? AND follower_alias_id = ? LIMIT 1";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(aliasId, followerAliasId).SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return rs.Any();
    }



    public async Task<IReadOnlyList<Follower>> GetAllFollowersOfAliasAsync(Guid aliasId, CancellationToken ct)
    {
        //This query is now CORRECT because it uses the partition key 'alias_id' in the WHERE clause.
        var cql = @"SELECT alias_id, follower_alias_id, since FROM followers_of_alias WHERE alias_id = ?";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        
        //The binding now uses the correct parameter.
        var stmt = ps.Bind(aliasId).SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);

        var list = new List<Follower>(128);
        foreach (var row in rs)
        {
            var r = new FollowersOfAliasRow
            {
                AliasId = row.GetValue<Guid>("alias_id"),
                FollowerAliasId = row.GetValue<Guid>("follower_alias_id"),
                Since = row.GetValue<DateTimeOffset?>("since")
            };
            list.Add(FollowerTrackingMapper.ToDomain(r));
        }

        return list;
    }
}
