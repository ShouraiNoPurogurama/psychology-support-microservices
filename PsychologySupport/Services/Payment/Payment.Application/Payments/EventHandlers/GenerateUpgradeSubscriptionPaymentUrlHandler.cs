using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Queries.Subscription;
using Mapster;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;

namespace Payment.Application.Payments.EventHandlers;

public class GenerateUpgradeSubscriptionPaymentUrlHandler : IConsumer<GenerateUpgradeSubscriptionPaymentUrlRequest>
{
    private readonly ISender _sender;
    private readonly ILogger<GenerateUpgradeSubscriptionPaymentUrlHandler> _logger;

    public GenerateUpgradeSubscriptionPaymentUrlHandler(ISender sender, ILogger<GenerateUpgradeSubscriptionPaymentUrlHandler> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GenerateUpgradeSubscriptionPaymentUrlRequest> context)
    {
        try
        {
            var dto = context.Message.Adapt<UpgradeSubscriptionDto>();

            (long? paymentCode, string paymentUrl) = dto.PaymentMethod switch
            {
                PaymentMethodName.VNPay => await HandleVnPay(dto),
                PaymentMethodName.PayOS => await HandlePayOS(dto),
                _ => throw new InvalidOperationException($"Unsupported payment method: {dto.PaymentMethod}")
            };

            await context.RespondAsync(new GenerateUpgradeSubscriptionPaymentUrlResponse(paymentCode,paymentUrl));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate upgrade subscription payment URL for message: {@Message}", context.Message);
            throw;
        }
    }

    private async Task<(long? PaymentCode, string PaymentUrl)> HandleVnPay(UpgradeSubscriptionDto dto)
    {
        var vnPayCommand = new CreateVnPayCallBackUrlForUpgradeSubscriptionCommand(dto);
        var vnPayResult = await _sender.Send(vnPayCommand);
        return (null, vnPayResult.Url);
    }

    private async Task<(long? PaymentCode, string PaymentUrl)> HandlePayOS(UpgradeSubscriptionDto dto)
    {
        var payOsCommand = new CreatePayOSCallBackUrlForUpgradeSubscriptionCommand(dto);
        var payOsResult = await _sender.Send(payOsCommand);
        return (payOsResult.PaymentCode, payOsResult.Url);
    }
}
