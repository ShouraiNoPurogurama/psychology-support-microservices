namespace Billing.API.Abstractions
{
    public interface IRedisCache
    {
        Task<T?> GetCacheDataAsync<T>(string key);
        Task<object> RemoveCacheDataAsync(string key);
        Task<bool> SetCacheDataAsync<T>(string key, T value, TimeSpan expiration);
    }
}
