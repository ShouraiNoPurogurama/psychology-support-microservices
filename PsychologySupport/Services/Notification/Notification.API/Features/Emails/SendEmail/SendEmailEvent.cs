namespace Notification.API.Features.Emails.SendEmail;

public record SendEmailEvent(
    Guid EventId,
    string To,
    string Subject,
    string Body) : DomainEvent(EventId);