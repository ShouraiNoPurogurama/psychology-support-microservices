using Net.payOS.Types;
using Payment.Application.Payments.Dtos;

public interface IPayOSService
{
    Task<string> CreatePayOSUrlForSubscriptionAsync(BuySubscriptionDto dto, Guid paymentId);
    Task<string> CreatePayOSUrlForUpgradeSubscriptionAsync(UpgradeSubscriptionDto dto, Guid paymentId);
    Task<string> CreatePayOSUrlForBookingAsync(BuyBookingDto dto, Guid paymentId);

    Task<PaymentLinkInformation> GetPaymentLinkInformationAsync(long orderCode);
    Task<PaymentLinkInformation> CancelPaymentLinkAsync(long orderCode, string? cancellationReason = null);

    Task<string> ConfirmWebhookAsync(string webhookUrl);
    Task<WebhookData> VerifyWebhookDataAsync(string webhookJson);

}