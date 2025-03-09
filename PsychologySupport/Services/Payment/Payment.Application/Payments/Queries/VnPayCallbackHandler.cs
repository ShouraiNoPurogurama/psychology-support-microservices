using System.Transactions;
using System.Web;
using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using Payment.Application.Payments.Dtos;
using Payment.Application.ServiceContracts;
using Payment.Domain.Enums;

namespace Payment.Application.Payments.Queries;

public record VnPayCallbackQuery(VnPayCallbackDto VnPayCallback) : IQuery<VnPayCallbackResult>;

public record VnPayCallbackResult(string CallbackUrl);

public class VnPayCallbackHandler(IVnPayService vnPayService) : IQueryHandler<VnPayCallbackQuery, VnPayCallbackResult>
{
    public async Task<VnPayCallbackResult> Handle(VnPayCallbackQuery request, CancellationToken cancellationToken)
    {
        var orderInfo = request.VnPayCallback.OrderInfo;
        var parameters = HttpUtility.ParseQueryString(orderInfo!);
        var patientId = Guid.Parse(parameters["PatientId"]);

        if (!Enum.TryParse(parameters["PaymentMethod"], true, out PaymentMethodName paymentMethodName))
        {
            throw new BadRequestException("Invalid payment method");
        }

        // Parse PaymentType
        if (!Enum.TryParse(parameters["PaymentType"], true, out PaymentType paymentType))
        {
            throw new BadRequestException("Invalid payment type");
        }

        //Handle payment success
        if (request.VnPayCallback.Success)
        {
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