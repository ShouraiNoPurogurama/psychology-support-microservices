using BuildingBlocks.Messaging.Events.Notification;
using BuildingBlocks.Messaging.Events.Subscription;
using Mapster;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.Payments.EventHandlers;

public class SubscriptionPaymentDetailFailedEventHandler(
    IPublishEndpoint publishEndpoint,
    ILogger<SubscriptionPaymentDetailFailedEventHandler> logger)
    : INotificationHandler<SubscriptionPaymentDetailFailedEvent>
{
    public async Task Handle(SubscriptionPaymentDetailFailedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("*** Domain Event handled: {DomainEvent}", notification.GetType());
        
        SubscriptionPaymentDetailFailedIntegrationEvent paymentDetailDetailFailedEvent =
            notification.Adapt<SubscriptionPaymentDetailFailedIntegrationEvent>();

        SendEmailIntegrationEvent sendEmailIntegrationEvent = new SendEmailIntegrationEvent(
            notification.PatientEmail,
            "Subscription Payment Failed",
            "Your subscription payment has failed. Please check your payment details and try again.");

        await publishEndpoint.Publish(paymentDetailDetailFailedEvent, cancellationToken);
        await publishEndpoint.Publish(sendEmailIntegrationEvent, cancellationToken);
    }
}