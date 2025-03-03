using Payment.Application.Dtos;

namespace Payment.Application.ServiceContracts;

public interface IVNPayService
{
    Task<string> CreateVNPayUrlAsync(BuySubscriptionDto dto);
    Task<bool> ValidateSignatureAsync(string queryString, string signature);
}