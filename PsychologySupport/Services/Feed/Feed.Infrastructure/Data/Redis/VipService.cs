using System;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
using Feed.Application.Abstractions.VipService;

namespace Feed.Infrastructure.Data.Redis;

public sealed class VipService(IConnectionMultiplexer redis) : IVipService
{
    private readonly IDatabase _database = redis.GetDatabase();
    
    private static string GetVipKey(Guid aliasId) => $"vip:{aliasId}";
    
    public async Task<bool> IsVipAsync(Guid aliasId, CancellationToken ct)
    {
        var key = GetVipKey(aliasId);
        var value = await _database.StringGetAsync(key);
        
        return value.HasValue && value == "1";
    }
    
    public async Task UpdateVipStatusAsync(Guid aliasId, bool isVip, CancellationToken ct)
    {
        var key = GetVipKey(aliasId);
        var value = isVip ? "1" : "0";
        
        await _database.StringSetAsync(key, value, TimeSpan.FromMinutes(15)); // TTL 15m as per requirements
    }
}
