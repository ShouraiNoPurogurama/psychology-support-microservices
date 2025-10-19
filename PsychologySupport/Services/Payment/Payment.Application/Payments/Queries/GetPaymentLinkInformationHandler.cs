using MediatR;
using Net.payOS.Types;
using BuildingBlocks.Exceptions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Data;
using Payment.Application.ServiceContracts;
using BuildingBlocks.Messaging.Events.Queries.Scheduling;
using BuildingBlocks.Messaging.Events.Queries.Subscription;
using Pii.API.Protos; 

namespace Payment.Application.Payments.Queries;

public record GetPaymentLinkInformationQuery(long PaymentCode) : IRequest<PaymentLinkInformation>;

public class GetPaymentLinkInformationHandler : IRequestHandler<GetPaymentLinkInformationQuery, PaymentLinkInformation>
{
    private readonly IPayOSService _payOSService;
    private readonly IPaymentDbContext _dbContext;
    private readonly IRequestClient<SubscriptionGetPromoAndGiftRequest> _subscriptionClient;
    private readonly IRequestClient<BookingGetPromoAndGiftRequestEvent> _bookingClient;
    private readonly PiiService.PiiServiceClient _piiClient;

    public GetPaymentLinkInformationHandler(
        IPayOSService payOSService,
        IPaymentDbContext dbContext,
        IRequestClient<SubscriptionGetPromoAndGiftRequest> subscriptionClient,
        IRequestClient<BookingGetPromoAndGiftRequestEvent> bookingClient,
        PiiService.PiiServiceClient piiClient 
    )
    {
        _payOSService = payOSService;
        _dbContext = dbContext;
        _subscriptionClient = subscriptionClient;
        _bookingClient = bookingClient;
        _piiClient = piiClient; 
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
                var subscriptionEvent = new SubscriptionGetPromoAndGiftRequest(payment.SubscriptionId.Value);
                var subResponse = await _subscriptionClient.GetResponse<SubscriptionGetPromoAndGiftResponse>(subscriptionEvent, cancellationToken);
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

            var tx = paymentInfo.transactions.FirstOrDefault();
            var amount = paymentInfo.amount;
            var reference = tx?.reference ?? "UNKNOWN";

            // Gọi gRPC tới PiiService để lấy subjectRef + email
            var piiResponse = await _piiClient.ResolvePersonInfoByPatientIdAsync(
                new ResolvePersonInfoByPatientIdRequest
                {
                    PatientId = payment.PatientProfileId.ToString()
                },
                cancellationToken: cancellationToken
            );

            var email = piiResponse.Email;
            var subjectRef = Guid.Parse(piiResponse.SubjectRef);

            payment.AddFailedPaymentDetail(
                subjectRef,
                PaymentDetail.Of(amount, reference),
                email,
                promotionCode,
                giftId
            );

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return paymentInfo;
    }
}
