using BuildingBlocks.CQRS;
using Notification.API.Abstractions;

namespace Notification.API.Features.Notifications.Commands.MarkRead;

public class MarkNotificationsReadCommandHandler : ICommandHandler<MarkNotificationsReadCommand, MarkNotificationsReadResult>
{
    private readonly INotificationRepository _repository;

    public MarkNotificationsReadCommandHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<MarkNotificationsReadResult> Handle(MarkNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var markedCount = await _repository.MarkReadAsync(request.NotificationIds, cancellationToken);
        return new MarkNotificationsReadResult(markedCount);
    }
}
