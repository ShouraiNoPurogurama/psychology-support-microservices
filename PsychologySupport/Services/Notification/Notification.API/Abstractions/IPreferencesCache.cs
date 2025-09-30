using Notification.API.Models.Notifications;

namespace Notification.API.Abstractions;

public interface IPreferencesCache
{
    Task<NotificationPreferences> GetOrDefaultAsync(Guid aliasId, CancellationToken cancellationToken = default);
    
    void Remove(Guid aliasId);
    
    void Clear();
}
