namespace BuildingBlocks.Messaging.Events.Notification;

public record SendEmailIntegrationEvent(
    string To,
    string Subject,
    string Body) : NotificationIntegrationEvent;