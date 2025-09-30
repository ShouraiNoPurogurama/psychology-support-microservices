using BuildingBlocks.CQRS;

namespace Notification.API.Features.Notifications.Commands.MarkRead;

public record MarkNotificationsReadCommand(
    Guid RecipientAliasId,
    List<Guid> NotificationIds
) : ICommand<MarkNotificationsReadResult>;

public record MarkNotificationsReadResult(int MarkedCount);
