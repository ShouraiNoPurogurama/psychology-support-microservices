using BuildingBlocks.Messaging.Events.Notification;
using Notification.API.Base.EventHandlers;
using Notification.API.Emails.Events;
using Notification.API.Outbox.Services;

namespace Notification.API.Emails.EventHandlers;

public class SendEmailNotificationIntegrationEventHandler(OutboxService outboxService) 
    : NotificationIntegrationEventHandler<SendEmailIntegrationEvent, SendEmailEvent>(outboxService)
{
    protected override SendEmailEvent ConvertToDomainEvent(SendEmailIntegrationEvent integrationEvent)
    {
        return new SendEmailEvent(integrationEvent.Id, integrationEvent.To, integrationEvent.Subject, integrationEvent.Body);
    }
}