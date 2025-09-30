using BuildingBlocks.CQRS;
using Notification.API.Contracts;

namespace Notification.API.Features.Notifications.Commands.MarkAllRead;

public class MarkAllNotificationsReadCommandHandler : ICommandHandler<MarkAllNotificationsReadCommand, MarkAllNotificationsReadResult>
{
    private readonly INotificationRepository _repository;

    public MarkAllNotificationsReadCommandHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<MarkAllNotificationsReadResult> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var cutoffTime = request.CutoffTime ?? DateTimeOffset.UtcNow;
        var markedCount = await _repository.MarkAllReadBeforeAsync(
            request.RecipientAliasId,
            cutoffTime,
            cancellationToken);

        return new MarkAllNotificationsReadResult(markedCount);
    }
}
