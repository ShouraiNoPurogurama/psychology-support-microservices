using System.Runtime.InteropServices;
using Microsoft.Extensions.Caching.Memory;
using Notification.API.Features.Notifications.Models;
using Notification.API.Features.Preferences.Models;
using Notification.API.Shared.Contracts;

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

    public async Task<List<NotificationPreferences>> GetOrDefaultAsync(List<Guid> aliasIds,
        CancellationToken cancellationToken = default)
    {
        if (!aliasIds.Any())
        {
            return [];
        }

        var distinctIds = aliasIds.Distinct().ToList();

        var prefsLookup = new Dictionary<Guid, NotificationPreferences>(distinctIds.Count);

        var nonCachedAliasIds = new List<Guid>();

        foreach (var aliasId in distinctIds)
        {
            var cacheKey = $"prefs:{aliasId}";

            if (_cache.TryGetValue<NotificationPreferences>(cacheKey, out var cached) && cached != null)
            {
                prefsLookup.Add(aliasId, cached);
            }
            else
            {
                nonCachedAliasIds.Add(aliasId);
            }
        }

        if (nonCachedAliasIds.Any())
        {
            var preferencesFromDb = await _repository.GetOrDefaultAsync(nonCachedAliasIds, cancellationToken);

            foreach (var pref in preferencesFromDb)
            {
                var cacheKey = $"prefs:{pref.Id}";
                _cache.Set(cacheKey, pref, _cacheDuration);
                prefsLookup.Add(pref.Id, pref);
            }
        }

        // Lặp qua list gốc (có trùng lặp) để trả về đúng số lượng và thứ tự
        var results = new List<NotificationPreferences>(aliasIds.Count);
        foreach (var aliasId in aliasIds)
        {
            if (prefsLookup.TryGetValue(aliasId, out var pref))
            {
                results.Add(pref);
            }
        }

        return results;
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