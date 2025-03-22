namespace BuildingBlocks.Messaging.Events.Notification;

public record SendMobilePushNotificationIntegrationEvent(IEnumerable<string> FCMToken,
    string Subject,
    string Body) : NotificationIntegrationEvent;