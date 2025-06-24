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
using Payment.Application.ServiceContracts;

namespace Payment.Application.Payments.Queries;

public record ProcessPayOSWebhookCommand(WebhookData WebhookData) : ICommand<ProcessPayOSWebhookResult>;

public record ProcessPayOSWebhookResult(bool Success);

public class ProcessPayOSWebhookHandler : ICommandHandler<ProcessPayOSWebhookCommand, ProcessPayOSWebhookResult>
{
    private readonly IPaymentDbContext _dbContext;
    private readonly IRequestClient<SubscriptionGetPromoAndGiftRequestEvent> _subscriptionClient;
    private readonly IRequestClient<BookingGetPromoAndGiftRequestEvent> _bookingClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPayOSService _payOSService;

    public ProcessPayOSWebhookHandler(
        IPaymentDbContext dbContext,
        IRequestClient<SubscriptionGetPromoAndGiftRequestEvent> subscriptionClient,
        IRequestClient<BookingGetPromoAndGiftRequestEvent> bookingClient,
        IHttpContextAccessor httpContextAccessor,
        IPayOSService payOSService)
    {
        _dbContext = dbContext;
        _subscriptionClient = subscriptionClient;
        _bookingClient = bookingClient;
        _httpContextAccessor = httpContextAccessor;
        _payOSService = payOSService;
    }

    public async Task<ProcessPayOSWebhookResult> Handle(ProcessPayOSWebhookCommand request, CancellationToken cancellationToken)
    {
        var webhookData = request.WebhookData;
        var paymentCode = webhookData.orderCode;

        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.PaymentCode == paymentCode, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Models.Payment), paymentCode);

        var amount = webhookData.amount;
        string? promotionCode = string.Empty;
        Guid? giftId = Guid.Empty;

        if (payment.SubscriptionId.HasValue && payment.SubscriptionId != Guid.Empty)
        {
            var subscriptionEvent = new SubscriptionGetPromoAndGiftRequestEvent(payment.SubscriptionId.Value);
            var subResponse = await _subscriptionClient.GetResponse<SubscriptionGetPromoAndGiftResponseEvent>(subscriptionEvent, cancellationToken);
            promotionCode = subResponse.Message.PromoCode;
            giftId = subResponse.Message.GiftId;
        }
        else if (payment.BookingId.HasValue && payment.BookingId != Guid.Empty)
        {
            var bookingEvent = new BookingGetPromoAndGiftRequestEvent(payment.BookingId.Value);
            var bookingResponse = await _bookingClient.GetResponse<BookingGetPromoAndGiftResponseEvent>(bookingEvent, cancellationToken);
            promotionCode = bookingResponse.Message.PromoCode;
            giftId = bookingResponse.Message.GiftId;
        }
        else
        {
            throw new InvalidOperationException("Payment must have either a subscription or a booking associated.");
        }

        var paymentInfo = await _payOSService.GetPaymentLinkInformationAsync(paymentCode);
        var status = paymentInfo.status.ToLowerInvariant(); // e.g., "paid", "cancelled", "failed", ...

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var email = GetEmailFromToken();

            switch (status)
            {
                case "paid":
                    payment.AddPaymentDetail(
                        PaymentDetail.Of(amount, webhookData.reference).MarkAsSuccess()
                    );
                    payment.MarkAsCompleted(email);
                    break;

                case "cancelled":
                case "failed":
                case "expired":
                    payment.AddFailedPaymentDetail(
                        PaymentDetail.Of(amount, webhookData.reference),
                        email,
                        promotionCode,
                        giftId
                    );
                    break;

                default:
                    throw new BadRequestException($"Unhandled payment status from PayOS: {status}");
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            scope.Complete();
        }

        return new ProcessPayOSWebhookResult(true);
    }

    private string GetEmailFromToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null || !httpContext.Request.Headers.ContainsKey("Authorization"))
            return "unknown@example.com";

        var authHeader = httpContext.Request.Headers["Authorization"].ToString();
        if (!authHeader.StartsWith("Bearer ")) return "unknown@example.com";

        var token = authHeader["Bearer ".Length..];

        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(token)) return "unknown@example.com";

        var jwtToken = tokenHandler.ReadJwtToken(token);
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == "email");

        return emailClaim?.Value ?? "unknown@example.com";
    }
}
