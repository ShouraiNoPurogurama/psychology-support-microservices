using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Scheduling;
using BuildingBlocks.Messaging.Events.Subscription;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Net.payOS.Types;
using Payment.Application.Data;
using Payment.Application.ServiceContracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Transactions;

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
            throw new InvalidOperationException("Xử lý thanh toán thất bại. Giao dịch thanh toán phải liên kết với ít nhất một gói đăng ký hoặc một đặt lịch.");
        }

        var paymentInfo = await _payOSService.GetPaymentLinkInformationAsync(paymentCode);
        var status = paymentInfo.status.ToLowerInvariant(); // e.g., "paid", "cancelled", "failed", ...

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var email = GetEmailFromClaims();

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
                    throw new BadRequestException($"Trạng thái thanh toán từ PayOS không được xử lý: {{status}}");
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            scope.Complete();
        }

        return new ProcessPayOSWebhookResult(true);
    }

    private string GetEmailFromClaims()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user == null || !(user.Identity?.IsAuthenticated ?? false))
            return "unknown@example.com";

        var email = user.FindFirstValue(ClaimTypes.Email)
                 ?? user.FindFirstValue("email")
                 ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                 ?? user.FindFirstValue(ClaimTypes.Name);

        return string.IsNullOrWhiteSpace(email) ? "unknown@example.com" : email;
    }
}
