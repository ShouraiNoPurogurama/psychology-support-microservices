using RealtimeHub.API.Models;

namespace RealtimeHub.API.Services;

/// <summary>
/// Service interface for sending real-time notifications via SignalR
/// </summary>
public interface IRealtimeHubService
{
    /// <summary>
    /// Send a notification to a specific user
    /// </summary>
    Task SendNotificationToUserAsync(Guid aliasId, NotificationMessage notification, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a notification to multiple users
    /// </summary>
    Task SendNotificationToUsersAsync(List<Guid> aliasIds, NotificationMessage notification, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a reward notification to a specific user
    /// </summary>
    Task SendRewardNotificationToUserAsync(Guid aliasId, RewardNotificationMessage rewardNotification, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a reward failure notification to a specific user
    /// </summary>
    Task SendRewardFailedNotificationToUserAsync(Guid aliasId, RewardFailedNotificationMessage rewardFailedNotification, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a progress update to a specific user
    /// </summary>
    Task SendProgressUpdateToUserAsync(Guid aliasId, ProgressUpdateMessage progressUpdate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a generic message to a specific user
    /// </summary>
    Task SendMessageToUserAsync(Guid aliasId, string method, object message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a generic message to a group
    /// </summary>
    Task SendMessageToGroupAsync(string groupName, string method, object message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get the number of active connections
    /// </summary>
    int GetActiveConnectionCount();
    
    /// <summary>
    /// Check if a user is currently connected
    /// </summary>
    bool IsUserConnected(Guid aliasId);
}
