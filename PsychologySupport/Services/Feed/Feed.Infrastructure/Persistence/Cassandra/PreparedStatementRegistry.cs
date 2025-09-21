using Cassandra;
using System.Collections.Concurrent;

namespace Feed.Infrastructure.Persistence.Cassandra;

public interface IPreparedStatementRegistry
{
    Task<PreparedStatement> GetStatementAsync(string key, string cql);
}

public sealed class PreparedStatementRegistry : IPreparedStatementRegistry
{
    private readonly ISession _session;
    private readonly ConcurrentDictionary<string, Task<PreparedStatement>> _statements = new();

    public PreparedStatementRegistry(ISession session)
    {
        _session = session;
    }

    public async Task<PreparedStatement> GetStatementAsync(string key, string cql)
    {
        return await _statements.GetOrAdd(key, async _ => await _session.PrepareAsync(cql));
    }
}
