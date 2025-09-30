using Notification.API.Models.Notifications;

namespace Notification.API.Abstractions;

public interface INotificationPreferencesRepository
{
    Task<NotificationPreferences?> GetByIdAsync(Guid aliasId, CancellationToken cancellationToken = default);
    
    Task<NotificationPreferences> GetOrDefaultAsync(Guid aliasId, CancellationToken cancellationToken = default);
    
    Task UpsertAsync(NotificationPreferences preferences, CancellationToken cancellationToken = default);
}
