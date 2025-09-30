using BuildingBlocks.CQRS;

namespace Notification.API.Features.Notifications.Queries.GetUnreadCount;

public record GetUnreadCountQuery(Guid RecipientAliasId) : IQuery<GetUnreadCountResult>;

public record GetUnreadCountResult(int Count);
