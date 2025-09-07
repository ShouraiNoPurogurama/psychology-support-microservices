using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using MassTransit;
using Notification.API.Domains.Outbox.Services;
using Notification.API.Models;

namespace Notification.API.Base.EventHandlers;

/// <summary>
/// Handles the processing of integration events within the module.
/// Ensures that messages are not duplicated by checking the outbox 
/// before adding new events for processing.
/// </summary>
public class NotificationIntegrationEventHandler<TNotificationEvent, TDomainEvent>(OutboxService outboxService)
    : IConsumer<TNotificationEvent>
    where TNotificationEvent : NotificationIntegrationEvent
    where TDomainEvent : DomainEvent
{
    public virtual async Task Consume(ConsumeContext<TNotificationEvent> context)
    {
        if (await outboxService.HasMessageAsync(context.Message, context.CancellationToken))
            return;

        //Always store as Domain events so that they can be
        //handled by other notification aggregates locally 
        var domainEvent = ConvertToDomainEvent(context.Message);

        var outboxMessage = OutboxMessage.Create(domainEvent);

        await outboxService.AddAsync(outboxMessage, context.CancellationToken);
    }

    //Always use the override logics
    protected virtual TDomainEvent ConvertToDomainEvent(TNotificationEvent integrationEvent)
    {
        return null!;
    }
}