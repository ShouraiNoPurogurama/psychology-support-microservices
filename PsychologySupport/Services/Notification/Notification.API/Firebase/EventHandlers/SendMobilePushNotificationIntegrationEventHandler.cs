using BuildingBlocks.Messaging.Events.Notification;
using Notification.API.Base.EventHandlers;
using Notification.API.Firebase.Events;
using Notification.API.Outbox.Services;

namespace Notification.API.Firebase.EventHandlers;

public class SendMobilePushNotificationIntegrationEventHandler(OutboxService outboxService)
    : NotificationIntegrationEventHandler<SendMobilePushNotificationIntegrationEvent, SendMobilePushNotificationEvent>(
        outboxService)
{
    protected override SendMobilePushNotificationEvent ConvertToDomainEvent(SendMobilePushNotificationIntegrationEvent integrationEvent)
    {
        return new SendMobilePushNotificationEvent(integrationEvent.Id, integrationEvent.FCMToken, integrationEvent.Subject, integrationEvent.Body);
    }
}