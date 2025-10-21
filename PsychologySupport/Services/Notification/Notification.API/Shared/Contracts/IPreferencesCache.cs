using Notification.API.Features.Preferences.Models;

namespace Notification.API.Shared.Contracts;

public interface IPreferencesCache
{
    Task<NotificationPreferences> GetOrDefaultAsync(Guid aliasId, CancellationToken cancellationToken = default);
    
    Task<List<NotificationPreferences>> GetOrDefaultAsync(List<Guid> aliasIds, CancellationToken cancellationToken = default);
    
    void Remove(Guid aliasId);
    
    void Clear();
}
