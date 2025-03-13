using BuildingBlocks.Messaging.Events.Notification;
using BuildingBlocks.Messaging.Events.Subscription;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.Payments.EventHandlers;

public class SubscriptionPaymentCompletedEventHandler(
    ILogger<SubscriptionPaymentCompletedEventHandler> logger,
    IPublishEndpoint publishEndpoint
) : INotificationHandler<SubscriptionPaymentDetailCompletedEvent>
{
    public async Task Handle(SubscriptionPaymentDetailCompletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("*** Handling SubscriptionPaymentDetailCompletedEvent for SubscriptionId: {SubscriptionId}", notification.SubscriptionId);
        
        var activateSubscriptionEvent = new SubscriptionPaymentSuccessIntegrationEvent(notification.SubscriptionId);
        
        var sendEmailEvent = new SendEmailIntegrationEvent(notification.PatientEmail, "Subscription Activated",
            "Your subscription has been activated successfully.");

        await publishEndpoint.Publish(activateSubscriptionEvent, cancellationToken);
        await publishEndpoint.Publish(sendEmailEvent, cancellationToken);
    }
}