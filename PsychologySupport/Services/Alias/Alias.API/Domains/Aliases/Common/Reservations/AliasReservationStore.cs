using StackExchange.Redis;

namespace Alias.API.Domains.Aliases.Common.Reservations;

public class AliasReservationStore(IConnectionMultiplexer mux) : IAliasReservationStore
{
    // key: alias:reserve:{aliasKey} -> value: aliasLabel
    private const string Prefix = "alias:reserve:";

    public async Task<bool> ReserveAsync(string aliasKey, string aliasLabel, TimeSpan ttl, CancellationToken cancellationToken)
    {
        var db = mux.GetDatabase();

        return await db.StringSetAsync(Prefix + aliasKey, aliasLabel, ttl, When.NotExists);
    }

    public async Task<(bool ok, string? label)> TryConsumeAsync(string aliasKey, CancellationToken ct)
    {
        var db = mux.GetDatabase();
        var key = Prefix + aliasKey;

        var val = await db.StringGetDeleteAsync(key);
        
        return (val.HasValue, val.HasValue ? (string?)val : null);
    }
    
    public async Task<bool> RemoveAsync(string aliasKey, CancellationToken ct)
    {
        var db = mux.GetDatabase();
        return await db.KeyDeleteAsync(Prefix + aliasKey);
    }

    public async Task<bool> ExistsAsync(string aliasKey, CancellationToken ct)
    {
        var db = mux.GetDatabase();
        return await db.KeyExistsAsync(Prefix + aliasKey);
    }
}