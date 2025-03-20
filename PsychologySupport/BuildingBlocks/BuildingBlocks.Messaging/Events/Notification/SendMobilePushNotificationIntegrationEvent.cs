namespace BuildingBlocks.Messaging.Events.Notification;

public record SendMobilePushNotificationIntegrationEvent(string FCMToken,
    string Subject,
    string Body) : NotificationIntegrationEvent;