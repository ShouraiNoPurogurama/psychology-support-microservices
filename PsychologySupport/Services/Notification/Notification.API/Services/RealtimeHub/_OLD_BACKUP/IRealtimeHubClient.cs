using Notification.API.Features.Notifications.Models;

namespace Notification.API.Services.RealtimeHub;

/// <summary>
/// Client for communicating with the RealtimeHub service
/// </summary>
public interface IRealtimeHubClient
{
    /// <summary>
    /// Send a real-time notification to a specific user
    /// </summary>
    Task SendNotificationToUserAsync(Guid aliasId, UserNotification notification, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a real-time notification to multiple users
    /// </summary>
    Task SendNotificationToUsersAsync(List<Guid> aliasIds, UserNotification notification, CancellationToken cancellationToken = default);
}
