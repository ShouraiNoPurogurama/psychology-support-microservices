using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Queries.Subscription;
using Mapster;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;

namespace Payment.Application.Payments.EventHandlers;

public class GenerateSubscriptionPaymentUrlHandler : IConsumer<GenerateSubscriptionPaymentUrlRequest>
{
    private readonly ISender _sender;
    private readonly ILogger<GenerateSubscriptionPaymentUrlHandler> _logger;

    public GenerateSubscriptionPaymentUrlHandler(ISender sender, ILogger<GenerateSubscriptionPaymentUrlHandler> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GenerateSubscriptionPaymentUrlRequest> context)
    {
        try
        {
            var dto = context.Message.Adapt<BuySubscriptionDto>();

            (long? paymentCode, string paymentUrl) = dto.PaymentMethod switch
            {
                PaymentMethodName.VNPay => await HandleVnPay(dto),
                PaymentMethodName.PayOS => await HandlePayOS(dto),
                _ => throw new InvalidOperationException($"Unsupported payment method: {dto.PaymentMethod}")
            };

            await context.RespondAsync(new GenerateSubscriptionPaymentUrlResponse(paymentCode,paymentUrl));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate subscription payment URL for message: {@Message}", context.Message);
            throw;
        }
    }

    private async Task<(long? PaymentCode, string PaymentUrl)> HandleVnPay(BuySubscriptionDto dto)
    {
        var vnPayCommand = new CreateVnPayCallBackUrlForSubscriptionCommand(dto);
        var vnPayResult = await _sender.Send(vnPayCommand);
        return (null, vnPayResult.Url);
    }

    private async Task<(long? PaymentCode, string PaymentUrl)> HandlePayOS(BuySubscriptionDto dto)
    {
        var payOsCommand = new CreatePayOSCallBackUrlForSubscriptionCommand(dto);
        var payOsResult = await _sender.Send(payOsCommand);
        return (payOsResult.PaymentCode, payOsResult.Url);
    }
}
