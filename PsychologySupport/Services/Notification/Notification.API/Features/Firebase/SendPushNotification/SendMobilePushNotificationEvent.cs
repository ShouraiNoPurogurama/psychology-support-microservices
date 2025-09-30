namespace Notification.API.Features.Firebase.SendPushNotification;

public record SendMobilePushNotificationEvent(Guid EventId,
    IEnumerable<string> FCMToken,
    string Subject,
    string Body) : DomainEvent(EventId);