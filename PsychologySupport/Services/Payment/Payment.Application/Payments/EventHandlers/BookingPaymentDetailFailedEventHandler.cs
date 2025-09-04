using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Payment;
using BuildingBlocks.Messaging.Events.Queries.Profile;
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
            "Thanh toán Booking thất bại",
            "Thanh toán cho Booking thất bại. Vui lòng kiểm tra lại thông tin thanh toán và thử lại.");
            
        var userDataResponse =
            await authClient.GetResponse<GetUserDataResponse>(new GetUserDataRequest(null, notification.PatientEmail),
                cancellationToken);

        var FCMTokens = userDataResponse.Message.FCMTokens;
        
        if (FCMTokens.Any())
        {
            var sendMobilePushNotificationEvent = new SendMobilePushNotificationIntegrationEvent(
                FCMTokens,             "Thanh toán Booking thất bại",
                "Thanh toán cho Booking thất bại. Vui lòng kiểm tra lại thông tin thanh toán và thử lại.");
            
            await publishEndpoint.Publish(sendMobilePushNotificationEvent, cancellationToken);
        }
        
        await publishEndpoint.Publish(paymentDetailFailedEvent, cancellationToken);
        await publishEndpoint.Publish(sendEmailIntegrationEvent, cancellationToken);
    }
}