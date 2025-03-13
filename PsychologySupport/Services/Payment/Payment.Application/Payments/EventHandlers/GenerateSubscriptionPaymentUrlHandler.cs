using BuildingBlocks.Messaging.Events.Payment;
using Mapster;
using MassTransit;
using MediatR;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;

namespace Payment.Application.Payments.EventHandlers;

public class GenerateSubscriptionPaymentUrlHandler(ISender sender) : IConsumer<GenerateSubscriptionPaymentUrlRequest>
{
    public async Task Consume(ConsumeContext<GenerateSubscriptionPaymentUrlRequest> context)
    {
        var dto = context.Message.Adapt<BuySubscriptionDto>();

        var command = new CreateVnPayCallBackUrlForSubscriptionCommand(dto);
        
        var result = await sender.Send(command);

        await context.RespondAsync(new GenerateSubscriptionPaymentUrlResponse(result.Url));
    }
}