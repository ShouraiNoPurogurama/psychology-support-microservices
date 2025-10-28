namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;

/// <summary>
/// Published when a reward notification is created.
/// Consumed specifically by RealtimeHub for real-time reward notification delivery.
/// </summary>
public record RewardNotificationCreatedEvent(
    Guid NotificationId,
    Guid RecipientAliasId,
    Guid RewardId,
    Guid SessionId,
    string StickerUrl,
    string PromptFilter,
    string Snippet,
    DateTimeOffset CreatedAt
) : NotificationIntegrationEvent;
