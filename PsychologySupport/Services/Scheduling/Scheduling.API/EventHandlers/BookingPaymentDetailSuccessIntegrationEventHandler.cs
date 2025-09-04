using BuildingBlocks.Messaging.Events.IntegrationEvents.Payment;
using MassTransit;
using MediatR;
using Scheduling.API.Enums;
using Scheduling.API.Features.UpdateBookingStatus;

namespace Scheduling.API.EventHandlers;

public class BookingPaymentDetailSuccessIntegrationEventHandler(ISender sender) : IConsumer<BookingPaymentDetailSuccessIntegrationEvent>
{
    public async Task Consume(ConsumeContext<BookingPaymentDetailSuccessIntegrationEvent> context)
    {
        await sender.Send(new UpdateBookingStatusCommand(context.Message.BookingId, BookingStatus.AwaitMeeting));
    }
}