using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using Notification.API.Features.Emails.SendEmail;
using Notification.API.Infrastructure.Outbox;
using Notification.API.Shared.Base.EventHandlers;

namespace Notification.API.Features.Emails.Consumers;

public class SendEmailNotificationIntegrationEventHandler(OutboxService outboxService) 
    : NotificationIntegrationEventHandler<SendEmailIntegrationEvent, SendEmailEvent>(outboxService)
{
    protected override SendEmailEvent ConvertToDomainEvent(SendEmailIntegrationEvent integrationEvent)
    {
        return new SendEmailEvent(integrationEvent.Id, integrationEvent.To, integrationEvent.Subject, integrationEvent.Body);
    }
}