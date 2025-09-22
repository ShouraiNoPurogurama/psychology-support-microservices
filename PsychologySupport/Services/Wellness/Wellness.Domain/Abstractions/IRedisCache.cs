using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wellness.Domain.Abstractions
{
    public interface IRedisCache
    {
        Task<T?> GetCacheDataAsync<T>(string key);
        Task<object> RemoveCacheDataAsync(string key);
        Task<bool> SetCacheDataAsync<T>(string key, T value, TimeSpan expiration);
    }
}
