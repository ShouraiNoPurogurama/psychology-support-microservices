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
    : INotificationHandler<PaymentDetailFailedEvent>
{
    public async Task Handle(PaymentDetailFailedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("*** Domain Event handled: {DomainEvent}", notification.GetType());

        SubscriptionPaymentFailedIntegrationEvent paymentDetailFailedEvent =
            notification.Adapt<SubscriptionPaymentFailedIntegrationEvent>();

        SendEmailIntegrationEvent sendEmailIntegrationEvent = new SendEmailIntegrationEvent(
            notification.PatientEmail,
            "Subscription Payment Failed",
            "Your subscription payment has failed. Please check your payment details and try again.");

        await publishEndpoint.Publish(paymentDetailFailedEvent, cancellationToken);
        await publishEndpoint.Publish(sendEmailIntegrationEvent, cancellationToken);
    }
}