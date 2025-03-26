using BuildingBlocks.Messaging.Events.Payment;
using Mapster;
using MassTransit;
using MediatR;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;

namespace Payment.Application.Payments.EventHandlers;

public class GenerateUpgradeSubscriptionPaymentUrlHandler(ISender sender) : IConsumer<GenerateUpgradeSubscriptionPaymentUrlRequest>
{
    public async Task Consume(ConsumeContext<GenerateUpgradeSubscriptionPaymentUrlRequest> context)
    {
        var dto = context.Message.Adapt<UpgradeSubscriptionDto>();

        var command = new CreateVnPayCallBackUrlForUpgradeSubscriptionCommand(dto);
        
        var result = await sender.Send(command);

        await context.RespondAsync(new GenerateUpgradeSubscriptionPaymentUrlResponse(result.Url));
    }
}