namespace Notification.API.Firebase.Events;

public record SendMobilePushNotificationEvent(Guid EventId,
    string FCMToken,
    string Subject,
    string Body) : DomainEvent(EventId);