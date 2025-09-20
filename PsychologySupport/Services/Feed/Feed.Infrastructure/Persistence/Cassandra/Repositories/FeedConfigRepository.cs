using Cassandra;
using Cassandra.Mapping;
using Feed.Application.Abstractions.FeedConfiguration;
using Feed.Domain.FeedConfiguration;
using Feed.Infrastructure.Persistence.Cassandra.Mappings;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Repositories;

public sealed class FeedConfigRepository : IFeedConfigRepository
{
    private readonly ISession _session;

    public FeedConfigRepository(ISession session)
    {
        _session = session;
    }

    public async Task<bool> SetAsync(FeedConfig config, CancellationToken ct)
    {
        var row = FeedConfigMapper.ToRow(config);
        var cql = @"INSERT INTO feed_config (key, value) VALUES (?, ?)";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(row.Key, row.Value).SetIdempotence(true);
        await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return true;
    }

    public async Task<FeedConfig?> GetAsync(string key, CancellationToken ct)
    {
        var cql = @"SELECT key, value FROM feed_config WHERE key = ?";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(key).SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);

        var row = rs.FirstOrDefault();
        if (row == null) return null;

        var configRow = new FeedConfigRow
        {
            Key = row.GetValue<string>("key"),
            Value = row.GetValue<string?>("value")
        };

        return FeedConfigMapper.ToDomain(configRow);
    }

    public async Task<bool> RemoveAsync(string key, CancellationToken ct)
    {
        var cql = @"DELETE FROM feed_config WHERE key = ?";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind(key).SetIdempotence(true);
        await _session.ExecuteAsync(stmt).ConfigureAwait(false);
        return true;
    }

    public async Task<IReadOnlyList<FeedConfig>> GetAllAsync(CancellationToken ct)
    {
        var cql = @"SELECT key, value FROM feed_config";
        var ps = await _session.PrepareAsync(cql).ConfigureAwait(false);
        var stmt = ps.Bind().SetIdempotence(true);
        var rs = await _session.ExecuteAsync(stmt).ConfigureAwait(false);

        var list = new List<FeedConfig>();
        foreach (var row in rs)
        {
            var configRow = new FeedConfigRow
            {
                Key = row.GetValue<string>("key"),
                Value = row.GetValue<string?>("value")
            };
            list.Add(FeedConfigMapper.ToDomain(configRow));
        }

        return list;
    }
}
