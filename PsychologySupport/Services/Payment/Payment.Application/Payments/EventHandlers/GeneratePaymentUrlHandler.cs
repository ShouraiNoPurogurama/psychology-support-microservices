using BuildingBlocks.Messaging.Events.Payment;
using Mapster;
using MassTransit;
using MediatR;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;

namespace Payment.Application.Payments.EventHandlers;

public class GeneratePaymentUrlHandler(ISender sender) : IConsumer<GeneratePaymentUrlRequest>
{
    public async Task Consume(ConsumeContext<GeneratePaymentUrlRequest> context)
    {
        var dto = context.Message.Adapt<BuySubscriptionDto>();

        var command = new CreateVnPayCallBackUrlForSubscriptionCommand(dto);
        
        var result = await sender.Send(command);

        await context.RespondAsync(new GeneratePaymentUrlResponse(result.Url));
    }
}