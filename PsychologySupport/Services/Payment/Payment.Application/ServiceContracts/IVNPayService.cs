using Payment.Application.Payment.Dtos;

namespace Payment.Application.ServiceContracts;

public interface IVNPayService
{
    Task<string> CreateVNPayUrlForSubscriptionAsync(BuySubscriptionDto dto);
    Task<bool> ValidateSignatureAsync(string queryString, string signature);
}