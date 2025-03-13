using Payment.Application.Payments.Dtos;

namespace Payment.Application.ServiceContracts;

public interface IVnPayService
{
    Task<string> CreateVNPayUrlForSubscriptionAsync(BuySubscriptionDto dto, Guid paymentId);
    Task<string> CreateVNPayUrlForBookingAsync(BuyBookingDto dto, Guid paymentId);
    Task<bool> ValidateSignatureAsync(string queryString, string signature);
}