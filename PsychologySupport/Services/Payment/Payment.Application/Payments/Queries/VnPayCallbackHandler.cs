using System.Transactions;
using System.Web;
using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.Notification;
using BuildingBlocks.Messaging.Events.Subscription;
using MassTransit;
using Payment.Application.Payments.Dtos;
using Payment.Application.ServiceContracts;
using Payment.Domain.Enums;

namespace Payment.Application.Payments.Queries;

public record VnPayCallbackQuery(VnPayCallbackDto VnPayCallback) : IQuery<VnPayCallbackResult>;

public record VnPayCallbackResult(string CallbackUrl);

public class VnPayCallbackHandler(IVnPayService vnPayService, IPublishEndpoint publishEndpoint)
    : IQueryHandler<VnPayCallbackQuery, VnPayCallbackResult>
{
    public async Task<VnPayCallbackResult> Handle(VnPayCallbackQuery request, CancellationToken cancellationToken)
    {
        var orderInfo = HttpUtility.UrlDecode(request.VnPayCallback.OrderInfo);
        var parameters = HttpUtility.ParseQueryString(orderInfo!);

        var dummy = Enum.TryParse(parameters[nameof(BuySubscriptionDto.PaymentMethod)], true, out PaymentMethodName paymentMethodName);
        
        if(paymentMethodName != PaymentMethodName.VNPay)
            throw new BadRequestException("Invalid payment method");

        if (!Enum.TryParse(parameters[nameof(BuySubscriptionDto.PaymentType)], true, out PaymentType paymentType))
        {
            throw new BadRequestException("Invalid payment type");
        }

        if (request.VnPayCallback.Success)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                switch (paymentType)
                {
                    case PaymentType.BuySubscription:
                        //TODO Activate subscription for patient by adjusting the status
                        //Send notification to patient Email and web account
                        var subscriptionId = Guid.Parse(parameters[nameof(BuySubscriptionDto.SubscriptionId)]);
                        var activateSubscriptionEvent = new ActivateSubscriptionIntegrationEvent(subscriptionId);
                        
                        var patientEmail = parameters[nameof(BuySubscriptionDto.PatientEmail)];
                        var sendEmailEvent = new SendEmailIntegrationEvent(patientEmail, "Subscription Activated",
                            "Your subscription has been activated successfully.");

                        await publishEndpoint.Publish(activateSubscriptionEvent, cancellationToken);
                        await publishEndpoint.Publish(sendEmailEvent, cancellationToken);
                        break;

                    case PaymentType.Booking:
                        //TODO Activate booking for patient by adjusting the status
                        //Send notification to patient Email and web account
                        break;
                }
            }
        }
        //Handle payment failure
        else if (!request.VnPayCallback.Success)
        {
            //TODO Re-enable promo code and gift code used by the patient
            //Delete user subscription if payment is expired
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                switch (paymentType)
                {
                    case PaymentType.BuySubscription:
                        //TODO Activate subscription for patient by adjusting the status
                        //Send notification to patient Email and web account
                        break;

                    case PaymentType.Booking:
                        //TODO Activate booking for patient by adjusting the status
                        //Send notification to patient Email and web account
                        break;
                }
            }
        }

        return new VnPayCallbackResult("https://localhost:5001");
    }
}