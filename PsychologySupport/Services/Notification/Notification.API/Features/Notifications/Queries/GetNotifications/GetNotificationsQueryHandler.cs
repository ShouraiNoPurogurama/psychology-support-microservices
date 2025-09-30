using BuildingBlocks.CQRS;
using Notification.API.Abstractions;
using Notification.API.Common;

namespace Notification.API.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsQueryHandler : IQueryHandler<GetNotificationsQuery, GetNotificationsResult>
{
    private readonly INotificationRepository _repository;

    public GetNotificationsQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetNotificationsResult> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        // Decode cursor
        var cursor = NotificationCursor.Decode(request.Cursor);
        var cursorCreatedAt = cursor?.CreatedAt;
        var cursorId = cursor?.Id;

        // Get paginated notifications
        var (items, hasMore) = await _repository.GetPagedAsync(
            request.RecipientAliasId,
            request.Limit,
            cursorCreatedAt,
            cursorId,
            request.Type,
            request.UnreadOnly,
            cancellationToken);

        // Get total unread count
        var totalCount = await _repository.GetUnreadCountAsync(request.RecipientAliasId, cancellationToken);

        // Map to DTOs
        var dtos = items.Select(n => new NotificationDto(
            n.Id,
            n.Type,
            n.IsRead,
            n.CreatedAt,
            n.ReadAt,
            n.ActorAliasId,
            n.ActorDisplayName,
            n.PostId,
            n.CommentId,
            n.ReactionId,
            n.Snippet
        )).ToList();

        // Generate next cursor
        string? nextCursor = null;
        if (hasMore && items.Any())
        {
            var lastItem = items.Last();
            nextCursor = NotificationCursor.Encode(lastItem.CreatedAt, lastItem.Id);
        }

        return new GetNotificationsResult(dtos, nextCursor, hasMore, totalCount);
    }
}
