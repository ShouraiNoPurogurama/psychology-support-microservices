namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;

public record SendMobilePushNotificationIntegrationEvent(IEnumerable<string> FCMToken,
    string Subject,
    string Body) : NotificationIntegrationEvent;