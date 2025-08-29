namespace Notification.API.Domains.Firebase.Events;

public record SendMobilePushNotificationEvent(Guid EventId,
    IEnumerable<string> FCMToken,
    string Subject,
    string Body) : DomainEvent(EventId);