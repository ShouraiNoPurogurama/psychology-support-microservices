namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;

/// <summary>
/// Published when a reward failure notification is created.
/// Consumed specifically by RealtimeHub for real-time reward failure notification delivery.
/// </summary>
public record RewardFailedNotificationCreatedEvent(
    Guid NotificationId,
    Guid RecipientAliasId,
    Guid RewardId,
    Guid SessionId,
    string ErrorMessage,
    int PointsRefunded,
    string Snippet,
    DateTimeOffset CreatedAt
) : NotificationIntegrationEvent;
