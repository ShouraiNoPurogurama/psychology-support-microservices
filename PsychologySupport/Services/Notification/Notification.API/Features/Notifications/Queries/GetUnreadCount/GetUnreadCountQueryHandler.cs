using BuildingBlocks.CQRS;
using Notification.API.Shared.Contracts;

namespace Notification.API.Features.Notifications.Queries.GetUnreadCount;

public class GetUnreadCountQueryHandler : IQueryHandler<GetUnreadCountQuery, GetUnreadCountResult>
{
    private readonly INotificationRepository _repository;

    public GetUnreadCountQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetUnreadCountResult> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _repository.GetUnreadCountAsync(request.RecipientAliasId, cancellationToken);
        return new GetUnreadCountResult(count);
    }
}
