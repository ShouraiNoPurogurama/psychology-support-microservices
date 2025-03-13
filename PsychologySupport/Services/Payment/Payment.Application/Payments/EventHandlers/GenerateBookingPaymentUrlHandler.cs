using BuildingBlocks.Messaging.Events.Payment;
using Mapster;
using MassTransit;
using MediatR;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;

namespace Payment.Application.Payments.EventHandlers;

public class GenerateBookingPaymentUrlHandler(ISender sender) : IConsumer<GenerateBookingPaymentUrlRequest>
{
    public async Task Consume(ConsumeContext<GenerateBookingPaymentUrlRequest> context)
    {
        var dto = context.Message.Adapt<BuyBookingDto>();

        var command = new CreateVnPayCallBackUrlForBookingCommand(dto);
        
        var result = await sender.Send(command);

        await context.RespondAsync(new GenerateBookingPaymentUrlResponse(result.Url));
    }
}