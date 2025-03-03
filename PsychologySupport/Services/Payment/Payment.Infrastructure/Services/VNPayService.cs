using Payment.Application.Dtos;
using Payment.Application.ServiceContracts;

namespace Payment.Infrastructure.Services;

public class VNPayService : IVNPayService
{
    public Task<string> CreateVNPayUrlAsync(BuySubscriptionDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ValidateSignatureAsync(string queryString, string signature)
    {
        throw new NotImplementedException();
    }
}