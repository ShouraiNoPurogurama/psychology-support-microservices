using StackExchange.Redis;
using System.Text.Json;

namespace Feed.Infrastructure.Data.Redis;

public interface IVipService
{
    Task<bool> IsVipAsync(Guid aliasId, CancellationToken ct);
    Task SetVipStatusAsync(Guid aliasId, bool isVip, TimeSpan? ttl = null, CancellationToken ct = default);
    Task InvalidateVipStatusAsync(Guid aliasId, CancellationToken ct = default);
}

public sealed class VipService(IConnectionMultiplexer redis) : IVipService
{
    private readonly IDatabase _database = redis.GetDatabase();
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(15);

    public async Task<bool> IsVipAsync(Guid aliasId, CancellationToken ct)
    {
        var key = GetVipKey(aliasId);
        var value = await _database.StringGetAsync(key);
        return value.HasValue && value == "1";
    }

    public async Task SetVipStatusAsync(Guid aliasId, bool isVip, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var key = GetVipKey(aliasId);
        var value = isVip ? "1" : "0";
        var expiry = ttl ?? DefaultTtl;
        
        await _database.StringSetAsync(key, value, expiry);
    }

    public async Task InvalidateVipStatusAsync(Guid aliasId, CancellationToken ct = default)
    {
        var key = GetVipKey(aliasId);
        await _database.KeyDeleteAsync(key);
    }

    private static string GetVipKey(Guid aliasId) => $"vip:{aliasId}";
}
