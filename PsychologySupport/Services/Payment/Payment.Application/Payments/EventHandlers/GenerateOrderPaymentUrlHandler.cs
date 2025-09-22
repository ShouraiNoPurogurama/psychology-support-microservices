using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Queries.Billing;
using BuildingBlocks.Messaging.Events.Queries.Subscription;
using Mapster;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;


namespace Payment.Application.Payments.EventHandlers
{
    public class GenerateOrderPaymentUrlHandler : IConsumer<GenerateOrderPaymentUrlRequest>
    {
        private readonly ISender _sender;
        private readonly ILogger<GenerateOrderPaymentUrlHandler> _logger;

        public GenerateOrderPaymentUrlHandler(ISender sender, ILogger<GenerateOrderPaymentUrlHandler> logger)
        {
            _sender = sender;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<GenerateOrderPaymentUrlRequest> context)
        {
            try
            {
                var dto = context.Message.Adapt<OrderDto>();

                (long? paymentCode, string paymentUrl) = dto.PaymentMethod switch
                {
                    PaymentMethodName.PayOS => await HandlePayOS(dto),
                    _ => throw new InvalidOperationException($"Unsupported payment method: {dto.PaymentMethod}")
                };

                await context.RespondAsync(new GenerateOrderPaymentUrlResponse(paymentCode, paymentUrl));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate order payment URL for message: {@Message}", context.Message);
                throw;
            }
        }

        private async Task<(long? PaymentCode, string PaymentUrl)> HandlePayOS(OrderDto dto)
        {
            var payOsCommand = new CreatePayOSCallBackUrlForOrderCommand(dto);
            var payOsResult = await _sender.Send(payOsCommand);
            return (payOsResult.PaymentCode, payOsResult.Url);
        }
    }
}
