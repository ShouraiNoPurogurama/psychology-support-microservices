namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;

/// <summary>
/// Published when a notification is created and saved to the database.
/// Consumed by delivery services (RealtimeHub, Email, Firebase) for sending.
/// </summary>
public record NotificationCreatedIntegrationEvent(
    Guid NotificationId,
    Guid RecipientAliasId,
    Guid? ActorAliasId,
    string ActorDisplayName,
    string NotificationType,
    bool IsRead,
    DateTimeOffset? ReadAt,
    Guid? PostId,
    Guid? CommentId,
    Guid? ReactionId,
    Guid? FollowId,
    Guid? GiftId,
    Guid? RewardId,
    Guid? SessionId,
    string? ModerationAction,
    string? Snippet,
    DateTimeOffset CreatedAt
) : NotificationIntegrationEvent;
