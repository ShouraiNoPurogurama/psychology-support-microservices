using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Scheduling;
using BuildingBlocks.Messaging.Events.Subscription;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Net.payOS.Types;
using Payment.Application.Data;
using System.Transactions;
using System.IdentityModel.Tokens.Jwt;

namespace Payment.Application.Payments.Queries;

public record ProcessPayOSWebhookCommand(WebhookData WebhookData) : ICommand<ProcessPayOSWebhookResult>;
public record ProcessPayOSWebhookResult(bool Success);

public class ProcessPayOSWebhookHandler : ICommandHandler<ProcessPayOSWebhookCommand, ProcessPayOSWebhookResult>
{
    private readonly IPaymentDbContext _dbContext;
    private readonly IRequestClient<SubscriptionGetPromoAndGiftRequestEvent> _subscriptionClient;
    private readonly IRequestClient<BookingGetPromoAndGiftRequestEvent> _bookingClient;

    public ProcessPayOSWebhookHandler(
        IPaymentDbContext dbContext,
        IRequestClient<SubscriptionGetPromoAndGiftRequestEvent> subscriptionClient,
        IRequestClient<BookingGetPromoAndGiftRequestEvent> bookingClient)
    {
        _dbContext = dbContext;
        _subscriptionClient = subscriptionClient;
        _bookingClient = bookingClient;
    }

    public async Task<ProcessPayOSWebhookResult> Handle(ProcessPayOSWebhookCommand request, CancellationToken cancellationToken)
    {
        var webhookData = request.WebhookData;
        var paymentCode = webhookData.orderCode;

        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.PaymentCode == paymentCode, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Models.Payment), paymentCode);

        var desc = webhookData.desc;
        var amount = webhookData.amount;
        string? promotionCode = string.Empty;
        Guid? giftId = Guid.Empty;

        if (payment.SubscriptionId.HasValue && payment.SubscriptionId != Guid.Empty)
        {
            var subscriptionGetPromoAndGiftEvent = new SubscriptionGetPromoAndGiftRequestEvent(payment.SubscriptionId.Value);
            var subResponse = await _subscriptionClient.GetResponse<SubscriptionGetPromoAndGiftResponseEvent>(subscriptionGetPromoAndGiftEvent, cancellationToken);
            promotionCode = subResponse.Message.PromoCode;
            giftId = subResponse.Message.GiftId;
        }
        else if (payment.BookingId.HasValue && payment.BookingId != Guid.Empty)
        {
            var bookingGetPromoAndGiftEvent = new BookingGetPromoAndGiftRequestEvent(payment.BookingId.Value);
            var bookingResponse = await _bookingClient.GetResponse<BookingGetPromoAndGiftResponseEvent>(bookingGetPromoAndGiftEvent, cancellationToken);
            promotionCode = bookingResponse.Message.PromoCode;
            giftId = bookingResponse.Message.GiftId;
        }
        else
        {
            throw new InvalidOperationException("Payment must have either a subscription or a booking associated.");
        }

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            if (desc.Contains("success"))
            {
                payment.AddPaymentDetail(
                    PaymentDetail.Of(amount, webhookData.reference).MarkAsSuccess()
                );
                payment.MarkAsCompleted("unknown@example.com");
            }
            else if (desc.Contains("cancelled") || desc.Contains("failed"))
            {
                payment.AddFailedPaymentDetail(
                    PaymentDetail.Of(amount, webhookData.reference),
                    "unknown@example.com",
                    promotionCode,
                    giftId
                );
            }
            else
            {
                throw new BadRequestException("Invalid payment status in webhook description");
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            scope.Complete();
        }

        return new ProcessPayOSWebhookResult(true);
    }
}