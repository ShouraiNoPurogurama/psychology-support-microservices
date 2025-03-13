using BuildingBlocks.Messaging.Events.Notification;
using BuildingBlocks.Messaging.Events.Scheduling;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Domain.Events;

namespace Payment.Application.Payments.EventHandlers;

public class BookingPaymentDetailCompletedEventHandler(IPublishEndpoint publishEndpoint,
    ILogger<BookingPaymentDetailCompletedEventHandler> logger) : INotificationHandler<BookingPaymentDetailCompletedEvent>
{
    public async Task Handle(BookingPaymentDetailCompletedEvent notification, CancellationToken cancellationToken)
    {
        //Update booking status
        //Update doctor availabilities
        //Send email to patient
        
        //Other stuffs related to doctor schedule
        
        logger.LogInformation("*** Handling BookingPaymentDetailCompletedEvent for BookingId: {BookingId}", notification.BookingId);
        
        var activateBookingEvent = new BookingPaymentDetailSuccessIntegrationEvent(notification.BookingId);
        
        var sendEmailEvent = new SendEmailIntegrationEvent(notification.PatientEmail, "Booking Activated",
            "Your booking has been paid successfully.");

        await publishEndpoint.Publish(activateBookingEvent, cancellationToken);
        await publishEndpoint.Publish(sendEmailEvent, cancellationToken);
        
    }
}