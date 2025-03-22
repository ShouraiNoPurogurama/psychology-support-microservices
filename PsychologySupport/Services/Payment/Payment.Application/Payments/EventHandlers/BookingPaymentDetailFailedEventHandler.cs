using BuildingBlocks.Messaging.Events.Auth;
using BuildingBlocks.Messaging.Events.Notification;
using BuildingBlocks.Messaging.Events.Scheduling;
using Mapster;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.Data;
using Payment.Domain.Events;

namespace Payment.Application.Payments.EventHandlers;

public class BookingPaymentDetailFailedEventHandler(
    IPublishEndpoint publishEndpoint,
    IRequestClient<GetUserDataRequest> authClient,
    ILogger<BookingPaymentDetailFailedEventHandler> logger) : INotificationHandler<BookingPaymentDetailFailedEvent>
{
    public async Task Handle(BookingPaymentDetailFailedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("*** Domain Event handled: {DomainEvent}", notification.GetType());
        
        BookingPaymentDetailFailedIntegrationEvent paymentDetailFailedEvent =
            notification.Adapt<BookingPaymentDetailFailedIntegrationEvent>();

        SendEmailIntegrationEvent sendEmailIntegrationEvent = new SendEmailIntegrationEvent(
            notification.PatientEmail,
            "Booking Payment Failed",
            "Your booking payment has failed. Please check your payment details and try again.");

        var userDataResponse =
            await authClient.GetResponse<GetUserDataResponse>(new GetUserDataRequest(null, notification.PatientEmail),
                cancellationToken);

        var FCMTokens = userDataResponse.Message.FCMTokens;
        
        if (FCMTokens.Any())
        {
            var sendMobilePushNotificationEvent = new SendMobilePushNotificationIntegrationEvent(
                FCMTokens, "Booking Payment Failed", "Your booking payment has failed. Please check your payment details and try again.");
            
            await publishEndpoint.Publish(sendMobilePushNotificationEvent, cancellationToken);
        }
        
        await publishEndpoint.Publish(paymentDetailFailedEvent, cancellationToken);
        await publishEndpoint.Publish(sendEmailIntegrationEvent, cancellationToken);
    }
}