using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Queries.Scheduling;
using Mapster;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;

namespace Payment.Application.Payments.EventHandlers;

public class GenerateBookingPaymentUrlHandler : IConsumer<GenerateBookingPaymentUrlRequest>
{
    private readonly ISender _sender;
    private readonly ILogger<GenerateBookingPaymentUrlHandler> _logger;

    public GenerateBookingPaymentUrlHandler(ISender sender, ILogger<GenerateBookingPaymentUrlHandler> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GenerateBookingPaymentUrlRequest> context)
    {
        try
        {
            var dto = context.Message.Adapt<BuyBookingDto>();

            string paymentUrl = dto.PaymentMethod switch
            {
                PaymentMethodName.VNPay => await HandleVnPay(dto),
                PaymentMethodName.PayOS => await HandlePayOS(dto),
                _ => throw new InvalidOperationException($"Unsupported payment method: {dto.PaymentMethod}")
            };

            await context.RespondAsync(new GenerateBookingPaymentUrlResponse(paymentUrl));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate booking payment URL for message: {@Message}", context.Message);
            throw;
        }
    }

    private async Task<string> HandleVnPay(BuyBookingDto dto)
    {
        var command = new CreateVnPayCallBackUrlForBookingCommand(dto);
        var result = await _sender.Send(command);
        return result.Url;
    }

    private async Task<string> HandlePayOS(BuyBookingDto dto)
    {
        var command = new CreatePayOSCallBackUrlForBookingCommand(dto);
        var result = await _sender.Send(command);
        return result.Url;
    }
}
