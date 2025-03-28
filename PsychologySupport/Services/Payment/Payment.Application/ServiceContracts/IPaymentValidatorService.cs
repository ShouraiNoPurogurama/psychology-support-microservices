using BuildingBlocks.Enums;
using Payment.Application.Payments.Dtos;

namespace Payment.Application.ServiceContracts;

public interface IPaymentValidatorService
{
    #region Validate Payment Methods

    void ValidateVNPayMethod(PaymentMethodName paymentMethod);
    
    #endregion

    #region Validate Payment Requests

    Task ValidateSubscriptionRequestAsync(BuySubscriptionDto dto);
    Task ValidateSubscriptionRequestAsync(UpgradeSubscriptionDto dto);
    Task ValidateBookingRequestAsync(BuyBookingDto dto);

    #endregion

    #region Common Validations

    

    #endregion
}