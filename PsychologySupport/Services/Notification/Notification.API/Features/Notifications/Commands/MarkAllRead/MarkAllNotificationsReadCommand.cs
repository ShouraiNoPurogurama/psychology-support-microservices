using BuildingBlocks.CQRS;

namespace Notification.API.Features.Notifications.Commands.MarkAllRead;

public record MarkAllNotificationsReadCommand(
    Guid RecipientAliasId,
    DateTimeOffset? CutoffTime = null
) : ICommand<MarkAllNotificationsReadResult>;

public record MarkAllNotificationsReadResult(int MarkedCount);
