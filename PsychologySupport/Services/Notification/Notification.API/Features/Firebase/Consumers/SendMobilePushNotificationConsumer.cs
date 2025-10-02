using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using Notification.API.Base.EventHandlers;
using Notification.API.Features.Firebase.SendPushNotification;
using Notification.API.Infrastructure.Outbox;

namespace Notification.API.Features.Firebase.Consumers;

public class SendMobilePushNotificationIntegrationEventHandler(OutboxService outboxService)
    : NotificationIntegrationEventHandler<SendMobilePushNotificationIntegrationEvent, SendMobilePushNotificationEvent>(
        outboxService)
{
    protected override SendMobilePushNotificationEvent ConvertToDomainEvent(SendMobilePushNotificationIntegrationEvent integrationEvent)
    {
        return new SendMobilePushNotificationEvent(integrationEvent.Id, integrationEvent.FCMToken, integrationEvent.Subject, integrationEvent.Body);
    }
}