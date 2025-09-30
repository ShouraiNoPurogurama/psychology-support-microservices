using BuildingBlocks.CQRS;
using Notification.API.Models.Notifications;

namespace Notification.API.Features.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery(
    Guid RecipientAliasId,
    int Limit = 20,
    string? Cursor = null,
    NotificationType? Type = null,
    bool UnreadOnly = false
) : IQuery<GetNotificationsResult>;

public record GetNotificationsResult(
    List<NotificationDto> Items,
    string? NextCursor,
    bool HasMore,
    int TotalCount
);

public record NotificationDto(
    Guid Id,
    NotificationType Type,
    bool IsRead,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt,
    Guid? ActorAliasId,
    string ActorDisplayName,
    Guid? PostId,
    Guid? CommentId,
    Guid? ReactionId,
    string? Snippet
);
