using Notification.API.Features.Notifications.Models;
using Notification.API.Features.Preferences.Models;

namespace Notification.API.Contracts;

public interface INotificationPreferencesRepository
{
    Task<NotificationPreferences?> GetByIdAsync(Guid aliasId, CancellationToken cancellationToken = default);
    
    Task<NotificationPreferences> GetOrDefaultAsync(Guid aliasId, CancellationToken cancellationToken = default);
    
    Task<List<NotificationPreferences>> GetOrDefaultAsync(List<Guid> aliasId, CancellationToken cancellationToken = default);
    
    Task UpsertAsync(NotificationPreferences preferences, CancellationToken cancellationToken = default);
}
