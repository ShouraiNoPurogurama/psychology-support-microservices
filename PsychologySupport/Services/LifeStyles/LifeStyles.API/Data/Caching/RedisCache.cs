using System.Text.Json;
using LifeStyles.API.Abstractions;
using StackExchange.Redis;

namespace LifeStyles.API.Data.Caching;

public class RedisCache : IRedisCache
{
    private IDatabase _database;

    public RedisCache(IConnectionMultiplexer connection)
    {
        _database = connection.GetDatabase();
    }

    public async Task<T> GetCacheDataAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);
        if (!string.IsNullOrEmpty(value))
        {
            return JsonSerializer.Deserialize<T>(value);
        }

        return default;
    }

    public async Task<object> RemoveCacheDataAsync(string key)
    {
        bool _keyExists = await _database.KeyExistsAsync(key);
        if (_keyExists)
        {
            return _database.KeyDelete(key);
        }
        return false;
    }

    public async Task<bool> SetCacheDataAsync<T>(string key, T value, TimeSpan expiration)
    {
        var jsonData = JsonSerializer.Serialize(value);
        return await _database.StringSetAsync(key, jsonData, expiration);
    }

}