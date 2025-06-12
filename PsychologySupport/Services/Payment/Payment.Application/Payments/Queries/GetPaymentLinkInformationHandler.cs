using MediatR;
using Net.payOS.Types;
using Payment.Domain.Models;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Scheduling;
using BuildingBlocks.Messaging.Events.Subscription;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Data;
using Mapster;

namespace Payment.Application.Payments.Queries;

public record GetPaymentLinkInformationQuery(long PaymentCode) : IRequest<PaymentLinkInformation>;

public class GetPaymentLinkInformationHandler : IRequestHandler<GetPaymentLinkInformationQuery, PaymentLinkInformation>
{
    private readonly IPayOSService _payOSService;
    private readonly IPaymentDbContext _dbContext;
    private readonly IRequestClient<SubscriptionGetPromoAndGiftRequestEvent> _subscriptionClient;
    private readonly IRequestClient<BookingGetPromoAndGiftRequestEvent> _bookingClient;

    public GetPaymentLinkInformationHandler(
        IPayOSService payOSService,
        IPaymentDbContext dbContext,
        IRequestClient<SubscriptionGetPromoAndGiftRequestEvent> subscriptionClient,
        IRequestClient<BookingGetPromoAndGiftRequestEvent> bookingClient)
    {
        _payOSService = payOSService;
        _dbContext = dbContext;
        _subscriptionClient = subscriptionClient;
        _bookingClient = bookingClient;
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

            payment.AddFailedPaymentDetail(
                PaymentDetail.Of(amount, reference),
                "system@app.com",
                promotionCode,
                giftId
            );

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return paymentInfo;
    }
}
