using Notification.API.Features.Notifications.Models;
using Notification.API.Features.Preferences.Models;

namespace Notification.API.Contracts;

public interface IPreferencesCache
{
    Task<NotificationPreferences> GetOrDefaultAsync(Guid aliasId, CancellationToken cancellationToken = default);
    
    void Remove(Guid aliasId);
    
    void Clear();
}
