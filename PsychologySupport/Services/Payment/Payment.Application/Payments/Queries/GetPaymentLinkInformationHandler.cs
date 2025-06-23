using MediatR;
using Net.payOS.Types;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Scheduling;
using BuildingBlocks.Messaging.Events.Subscription;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Data;
using Mapster;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using Payment.Application.ServiceContracts;

namespace Payment.Application.Payments.Queries;

public record GetPaymentLinkInformationQuery(long PaymentCode) : IRequest<PaymentLinkInformation>;

public class GetPaymentLinkInformationHandler : IRequestHandler<GetPaymentLinkInformationQuery, PaymentLinkInformation>
{
    private readonly IPayOSService _payOSService;
    private readonly IPaymentDbContext _dbContext;
    private readonly IRequestClient<SubscriptionGetPromoAndGiftRequestEvent> _subscriptionClient;
    private readonly IRequestClient<BookingGetPromoAndGiftRequestEvent> _bookingClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetPaymentLinkInformationHandler(
        IPayOSService payOSService,
        IPaymentDbContext dbContext,
        IRequestClient<SubscriptionGetPromoAndGiftRequestEvent> subscriptionClient,
        IRequestClient<BookingGetPromoAndGiftRequestEvent> bookingClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _payOSService = payOSService;
        _dbContext = dbContext;
        _subscriptionClient = subscriptionClient;
        _bookingClient = bookingClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PaymentLinkInformation> Handle(GetPaymentLinkInformationQuery request, CancellationToken cancellationToken)
    {
        var paymentInfo = await _payOSService.GetPaymentLinkInformationAsync(request.PaymentCode);

        if (paymentInfo.status == "CANCELLED" || paymentInfo.status == "EXPIRED")
        {
            var payment = await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.PaymentCode == request.PaymentCode, cancellationToken)
                ?? throw new NotFoundException(nameof(Payment.Domain.Models.Payment), request.PaymentCode);

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

            var tx = paymentInfo.transactions.FirstOrDefault();
            var amount = paymentInfo.amount;
            var reference = tx?.reference ?? "UNKNOWN";
            var email = GetEmailFromToken();

            payment.AddFailedPaymentDetail(
                PaymentDetail.Of(amount, reference),
                email,
                promotionCode,
                giftId
            );

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return paymentInfo;
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
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

        return emailClaim?.Value ?? "unknown@example.com";
    }
}
