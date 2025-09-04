using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using Notification.API.Base.EventHandlers;
using Notification.API.Domains.Firebase.Events;
using Notification.API.Domains.Outbox.Services;

namespace Notification.API.Domains.Firebase.EventHandlers;

public class SendMobilePushNotificationIntegrationEventHandler(OutboxService outboxService)
    : NotificationIntegrationEventHandler<SendMobilePushNotificationIntegrationEvent, SendMobilePushNotificationEvent>(
        outboxService)
{
    protected override SendMobilePushNotificationEvent ConvertToDomainEvent(SendMobilePushNotificationIntegrationEvent integrationEvent)
    {
        return new SendMobilePushNotificationEvent(integrationEvent.Id, integrationEvent.FCMToken, integrationEvent.Subject, integrationEvent.Body);
    }
}