using Notification.API.Features.Notifications.Models;

namespace Notification.API.Hubs;

public interface INotificationHubService
{
    Task SendNotificationToUserAsync(Guid aliasId, UserNotification notification, CancellationToken cancellationToken = default);
    Task SendNotificationToUsersAsync(List<Guid> aliasIds, UserNotification notification, CancellationToken cancellationToken = default);
}
