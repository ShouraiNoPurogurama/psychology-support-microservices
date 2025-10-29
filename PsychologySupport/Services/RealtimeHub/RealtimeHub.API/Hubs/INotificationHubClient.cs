using RealtimeHub.API.Models;

namespace RealtimeHub.API.Hubs;

/// <summary>
/// Defines the client-side methods that can be called from the NotificationHub
/// </summary>
public interface INotificationHubClient
{
    /// <summary>
    /// Sends a general notification to the client
    /// </summary>
    Task ReceiveNotification(NotificationMessage notification);

    /// <summary>
    /// Sends a reward notification to the client with additional reward-specific data
    /// </summary>
    Task ReceiveRewardNotification(RewardNotificationMessage rewardNotification);

    /// <summary>
    /// Sends a reward failure notification to the client when reward processing fails
    /// </summary>
    Task ReceiveRewardFailedNotification(RewardFailedNotificationMessage rewardFailedNotification);

    /// <summary>
    /// Sends a progress update to the client when they earn points
    /// </summary>
    Task ReceiveProgressUpdate(ProgressUpdateMessage progressUpdate);
}
