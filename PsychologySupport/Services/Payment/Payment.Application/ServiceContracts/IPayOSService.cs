using Payment.Application.Payments.Dtos;

public interface IPayOSService
{
    Task<string> CreatePayOSUrlForSubscriptionAsync(BuySubscriptionDto dto, Guid paymentId);
    Task<string> CreatePayOSUrlForUpgradeSubscriptionAsync(UpgradeSubscriptionDto dto, Guid paymentId);
    Task<string> CreatePayOSUrlForBookingAsync(BuyBookingDto dto, Guid paymentId);

    //bool ValidateSignature(string data, string signature);
}