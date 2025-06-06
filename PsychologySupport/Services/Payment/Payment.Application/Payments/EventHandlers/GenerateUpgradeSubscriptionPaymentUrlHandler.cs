using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Payment;
using Mapster;
using MassTransit;
using MediatR;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;

namespace Payment.Application.Payments.EventHandlers;

public class GenerateUpgradeSubscriptionPaymentUrlHandler : IConsumer<GenerateUpgradeSubscriptionPaymentUrlRequest>
{
    private readonly ISender _sender;

    public GenerateUpgradeSubscriptionPaymentUrlHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task Consume(ConsumeContext<GenerateUpgradeSubscriptionPaymentUrlRequest> context)
    {
        var dto = context.Message.Adapt<UpgradeSubscriptionDto>();

        string paymentUrl = dto.PaymentMethod switch
        {
            PaymentMethodName.VNPay => await HandleVnPay(dto),
            PaymentMethodName.PayOS => await HandlePayOS(dto),
            _ => throw new InvalidOperationException($"Unsupported payment method: {dto.PaymentMethod}")
        };

        await context.RespondAsync(new GenerateUpgradeSubscriptionPaymentUrlResponse(paymentUrl));
    }

    private async Task<string> HandleVnPay(UpgradeSubscriptionDto dto)
    {
        var vnPayCommand = new CreateVnPayCallBackUrlForUpgradeSubscriptionCommand(dto);
        var vnPayResult = await _sender.Send(vnPayCommand);
        return vnPayResult.Url;
    }

    private async Task<string> HandlePayOS(UpgradeSubscriptionDto dto)
    {
        var payOsCommand = new CreatePayOSCallBackUrlForUpgradeSubscriptionCommand(dto);
        var payOsResult = await _sender.Send(payOsCommand);
        return payOsResult.Url;
    }
}