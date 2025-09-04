namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;

public record SendEmailIntegrationEvent(
    string To,
    string Subject,
    string Body) : NotificationIntegrationEvent;