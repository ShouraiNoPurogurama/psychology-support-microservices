using BuildingBlocks.DDD;

namespace Notification.API.Emails.Events;

public record SendEmailEvent(
    Guid EventId,
    string To,
    string Subject,
    string Body) : DomainEvent(EventId);