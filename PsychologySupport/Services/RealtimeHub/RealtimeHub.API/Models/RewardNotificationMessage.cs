namespace RealtimeHub.API.Models;

/// <summary>
/// Reward notification message model for real-time delivery
/// Contains additional reward-specific information
/// </summary>
public record RewardNotificationMessage
{
    public Guid NotificationId { get; init; }
    public Guid RecipientAliasId { get; init; }
    public Guid RewardId { get; init; }
    public Guid SessionId { get; init; }
    public string StickerUrl { get; init; } = string.Empty;
    public string Snippet { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}
