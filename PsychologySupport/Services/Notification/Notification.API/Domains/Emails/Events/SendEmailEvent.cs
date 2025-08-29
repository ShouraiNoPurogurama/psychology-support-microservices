namespace Notification.API.Domains.Emails.Events;

public record SendEmailEvent(
    Guid EventId,
    string To,
    string Subject,
    string Body) : DomainEvent(EventId);