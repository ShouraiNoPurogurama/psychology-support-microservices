namespace RealtimeHub.API.Models;

/// <summary>
/// Reward failure notification message model for real-time delivery
/// Contains information about the failed reward and points refunded
/// </summary>
public record RewardFailedNotificationMessage
{
    public Guid NotificationId { get; init; }
    public Guid RecipientAliasId { get; init; }
    public Guid RewardId { get; init; }
    public Guid SessionId { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public int PointsRefunded { get; init; }
    public string Snippet { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}
