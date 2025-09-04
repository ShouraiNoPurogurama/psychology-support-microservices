using System.Transactions;
using System.Web;
using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using Mapster;
using MassTransit;
using Payment.Application.Data;
using Payment.Application.Payments.Dtos;

namespace Payment.Application.Payments.Queries;

public record VnPayCallbackQuery(VnPayCallbackDto VnPayCallback) : IQuery<VnPayCallbackResult>;

public record VnPayCallbackResult(PaymentDto Payment);

public class VnPayCallbackHandler(IPaymentDbContext dbContext)
    : IQueryHandler<VnPayCallbackQuery, VnPayCallbackResult>
{
    public async Task<VnPayCallbackResult> Handle(VnPayCallbackQuery request, CancellationToken cancellationToken)
    {
        var orderInfo = HttpUtility.UrlDecode(request.VnPayCallback.OrderInfo);
        var parameters = HttpUtility.ParseQueryString(orderInfo!);

        Enum.TryParse(parameters[nameof(BuySubscriptionDto.PaymentMethod)], true, out PaymentMethodName paymentMethodName);

        if (paymentMethodName != PaymentMethodName.VNPay)
            throw new BadRequestException("Invalid payment method");

        if (!Enum.TryParse(parameters[nameof(BuySubscriptionDto.PaymentType)], true, out PaymentType paymentType))
        {
            throw new BadRequestException("Invalid payment type");
        }

        var promoCode = string.IsNullOrWhiteSpace(parameters[nameof(BuySubscriptionDto.PromoCode)])
            ? null
            : parameters[nameof(BuySubscriptionDto.PromoCode)];

        Guid? giftCodeId = string.IsNullOrWhiteSpace(parameters[nameof(BuySubscriptionDto.GiftId)])
            ? null
            : Guid.Parse(parameters[nameof(BuySubscriptionDto.GiftId)]!);

        Guid? subscriptionId = string.IsNullOrWhiteSpace(parameters[nameof(BuySubscriptionDto.SubscriptionId)])
            ? null
            : Guid.Parse(parameters[nameof(BuySubscriptionDto.SubscriptionId)]!);

        var paymentId = Guid.Parse(parameters[nameof(Domain.Models.Payment.Id)]!);
        var patientEmail = parameters[nameof(BuySubscriptionDto.PatientEmail)];

        var finalPrice = 0.01m * request.VnPayCallback.Amount;
        var payment = await dbContext.Payments.FindAsync([paymentId], cancellationToken: cancellationToken)
                      ?? throw new NotFoundException("Payment", paymentId);

        if (request.VnPayCallback.Success)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                switch (paymentType)
                {
                    case PaymentType.BuySubscription:
                        //TODO
                        //Store payment details to db
                        //Activate subscription for patient by adjusting the status
                        //Send notification to patient Email and web account

                        payment.AddPaymentDetail(
                            PaymentDetail.Of(finalPrice, request.VnPayCallback.TransactionCode).MarkAsSuccess()
                        );

                        payment.MarkAsCompleted(patientEmail!);

                        await dbContext.SaveChangesAsync(cancellationToken);
                        break;

                    case PaymentType.Booking:
                        //TODO Activate booking for patient by adjusting the status
                        //Send notification to patient Email and web account
                        
                        payment.AddPaymentDetail(
                            PaymentDetail.Of(finalPrice, request.VnPayCallback.TransactionCode).MarkAsSuccess()
                        );

                        payment.MarkAsCompleted(patientEmail!);

                        await dbContext.SaveChangesAsync(cancellationToken);
                        break;
                    case PaymentType.UpgradeSubscription:
                        
                        //TODO deactivate old subscription
                        // activate new subscription
                        // send notification to patient Email and web account
                        // store payment details to db
                    
                        payment.AddPaymentDetail(
                            PaymentDetail.Of(finalPrice, request.VnPayCallback.TransactionCode).MarkAsSuccess()
                            );
                        
                        payment.MarkAsCompleted(patientEmail!);
                        await dbContext.SaveChangesAsync(cancellationToken);
                        break;
                }

                scope.Complete();
            }
        }
        //Handle payment failure
        else if (!request.VnPayCallback.Success)
        {
            //TODO Re-enable promo code and gift code used by the patient
            //Send notification to patient Email and web account
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var paymentDetail = PaymentDetail.Of(finalPrice, request.VnPayCallback.TransactionCode);

                payment.AddFailedPaymentDetail(
                    paymentDetail, patientEmail!, promoCode, giftCodeId
                );

                await dbContext.SaveChangesAsync(cancellationToken);

                scope.Complete();
            }
        }

        return new VnPayCallbackResult(payment.Adapt<PaymentDto>());
    }
}