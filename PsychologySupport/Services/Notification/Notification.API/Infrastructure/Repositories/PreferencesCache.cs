using Microsoft.Extensions.Caching.Memory;
using Notification.API.Contracts;
using Notification.API.Features.Notifications.Models;
using Notification.API.Features.Preferences.Models;

namespace Notification.API.Infrastructure.Repositories;

public class PreferencesCache : IPreferencesCache
{
    private readonly INotificationPreferencesRepository _repository;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(60);

    public PreferencesCache(
        INotificationPreferencesRepository repository,
        IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<NotificationPreferences> GetOrDefaultAsync(Guid aliasId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"prefs:{aliasId}";

        if (_cache.TryGetValue<NotificationPreferences>(cacheKey, out var cached) && cached != null)
        {
            return cached;
        }

        var preferences = await _repository.GetOrDefaultAsync(aliasId, cancellationToken);

        _cache.Set(cacheKey, preferences, _cacheDuration);

        return preferences;
    }

    public void Remove(Guid aliasId)
    {
        var cacheKey = $"prefs:{aliasId}";
        _cache.Remove(cacheKey);
    }

    public void Clear()
    {
        // MemoryCache doesn't have a built-in clear method
        // This is a limitation, but individual keys can be removed
    }
}
