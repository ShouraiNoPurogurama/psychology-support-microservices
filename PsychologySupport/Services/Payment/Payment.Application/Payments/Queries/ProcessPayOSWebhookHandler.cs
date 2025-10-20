using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Net.payOS.Types;
using Payment.Application.Data;
using Payment.Application.ServiceContracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Transactions;
using BuildingBlocks.Messaging.Events.Queries.Scheduling;
using BuildingBlocks.Messaging.Events.Queries.Subscription;
using Pii.API.Protos;

namespace Payment.Application.Payments.Queries;

public record ProcessPayOSWebhookCommand(WebhookData WebhookData) : ICommand<ProcessPayOSWebhookResult>;

public record ProcessPayOSWebhookResult(bool Success);

public class ProcessPayOSWebhookHandler : ICommandHandler<ProcessPayOSWebhookCommand, ProcessPayOSWebhookResult>
{
    private readonly IPaymentDbContext _dbContext;
    private readonly IRequestClient<SubscriptionGetPromoAndGiftRequest> _subscriptionClient;
    private readonly IRequestClient<BookingGetPromoAndGiftRequestEvent> _bookingClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPayOSService _payOSService;
    private readonly PiiService.PiiServiceClient _piiClient;

    public ProcessPayOSWebhookHandler(
        IPaymentDbContext dbContext,
        IRequestClient<SubscriptionGetPromoAndGiftRequest> subscriptionClient,
        IRequestClient<BookingGetPromoAndGiftRequestEvent> bookingClient,
        IPayOSService payOSService,
        PiiService.PiiServiceClient piiClient)
    {
        _dbContext = dbContext;
        _subscriptionClient = subscriptionClient;
        _bookingClient = bookingClient;
        _payOSService = payOSService;
        _piiClient = piiClient;
    }

    public async Task<ProcessPayOSWebhookResult> Handle(ProcessPayOSWebhookCommand request, CancellationToken cancellationToken)
    {
        var webhookData = request.WebhookData;
        var paymentCode = webhookData.orderCode;

        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.PaymentCode == paymentCode, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Models.Payment), paymentCode);

        var patientId = payment.PatientProfileId;

        // Gọi gRPC tới PiiService để lấy subjectRef + email
        var piiResponse = await _piiClient.ResolvePersonInfoByPatientIdAsync(
           new ResolvePersonInfoByPatientIdRequest
           {
               PatientId = patientId.ToString()
           },
           cancellationToken: cancellationToken
       );

        var subjectRef = Guid.Parse(piiResponse.SubjectRef);
        var email = piiResponse.Email ?? string.Empty;

        var amount = webhookData.amount;
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
        else
        {
            throw new InvalidOperationException("Xử lý thanh toán thất bại. Giao dịch thanh toán phải liên kết với ít nhất một gói đăng ký hoặc một đặt lịch.");
        }

        var paymentInfo = await _payOSService.GetPaymentLinkInformationAsync(paymentCode);
        var status = paymentInfo.status.ToLowerInvariant(); // e.g., "paid", "cancelled", "failed", ...

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
           

            switch (status)
            {
                case "paid":
                    payment.AddPaymentDetail(
                        PaymentDetail.Of(amount, webhookData.reference).MarkAsSuccess()
                    );
                    payment.MarkAsCompleted(subjectRef,email);
                    break;

                case "cancelled":
                case "failed":
                case "expired":
                    payment.AddFailedPaymentDetail(
                        subjectRef,
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

 
}
