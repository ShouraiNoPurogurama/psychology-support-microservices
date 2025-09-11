using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using YarpApiGateway.Features.TokenExchange;

namespace YarpApiGateway.Infrastructure;

public class CachingPiiLookupService : IPiiLookupService
{
    private readonly IPiiLookupService _decorated;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingPiiLookupService> _logger;

    public CachingPiiLookupService(GrpcPiiLookupService decorated, IDistributedCache cache, ILogger<CachingPiiLookupService> logger)
    {
        _decorated = decorated;
        _cache = cache;
        _logger = logger;
    }

    public Task<string?> ResolveAliasIdBySubjectRefAsync(string subjectRef)
    {
        string cacheKey = $"map:sub_alias:{subjectRef}";
        return GetOrSetCacheAsync(cacheKey, () => _decorated.ResolveAliasIdBySubjectRefAsync(subjectRef));
    }

    public Task<string?> ResolvePatientIdBySubjectRefAsync(string subjectRef)
    {
        string cacheKey = $"map:sub_patient:{subjectRef}";
        return GetOrSetCacheAsync(cacheKey, () => _decorated.ResolvePatientIdBySubjectRefAsync(subjectRef));
    }

    private async Task<string?> GetOrSetCacheAsync(string key, Func<Task<string?>> factory)
    {
        try
        {
            var cachedValue = await _cache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogInformation("Cache HIT for key: {Key}", key);
                return cachedValue;
            }

            _logger.LogInformation("Cache MISS for key: {Key}. Calling decorated service.", key);
            var realValue = await factory();

            if (!string.IsNullOrEmpty(realValue))
            {
                var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
                await _cache.SetStringAsync(key, realValue, options);
            }
            return realValue;
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis connection failed for key: {Key}. Falling back to factory.", key);
            return await factory();
        }
    }
}