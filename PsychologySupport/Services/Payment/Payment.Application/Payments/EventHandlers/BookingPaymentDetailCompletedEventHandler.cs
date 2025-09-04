using BuildingBlocks.Messaging.Events.IntegrationEvents.Notification;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Payment;
using BuildingBlocks.Messaging.Events.Queries.Profile;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.Payments.EventHandlers;

public class BookingPaymentDetailCompletedEventHandler(
    IPublishEndpoint publishEndpoint,
    IRequestClient<GetUserDataRequest> authClient,
    ILogger<BookingPaymentDetailCompletedEventHandler> logger) : INotificationHandler<BookingPaymentDetailCompletedEvent>
{
    public async Task Handle(BookingPaymentDetailCompletedEvent notification, CancellationToken cancellationToken)
    {
        //Update booking status
        //Update doctor availabilities
        //Send email to patient

        //Other stuffs related to doctor schedule

        logger.LogInformation("*** Handling BookingPaymentDetailCompletedEvent for BookingId: {BookingId}",
            notification.BookingId);

        var userDataResponse =
            await authClient.GetResponse<GetUserDataResponse>(new GetUserDataRequest(null, notification.PatientEmail),
                cancellationToken);

        var FCMTokens = userDataResponse.Message.FCMTokens;

        var activateBookingEvent = new BookingPaymentDetailSuccessIntegrationEvent(notification.BookingId);

        var sendEmailEvent = new SendEmailIntegrationEvent(notification.PatientEmail, "Booking đã được kích hoạt",
            "Booking của bạn đã được thanh toán thành công.");

        if (FCMTokens.Any())
        {
            var sendMobilePushNotificationEvent = new SendMobilePushNotificationIntegrationEvent(
                FCMTokens, "Booking đã được kích hoạt",
                "Booking của bạn đã được thanh toán thành công.");
            
            await publishEndpoint.Publish(sendMobilePushNotificationEvent, cancellationToken);
        }

        await publishEndpoint.Publish(activateBookingEvent, cancellationToken);
        await publishEndpoint.Publish(sendEmailEvent, cancellationToken);
    }
}