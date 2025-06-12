using Net.payOS.Types;
using Payment.Application.Payments.Dtos;

public interface IPayOSService
{
    Task<string> CreatePayOSUrlForSubscriptionAsync(BuySubscriptionDto dto, Guid paymentId,long paymentCode);
    Task<string> CreatePayOSUrlForUpgradeSubscriptionAsync(UpgradeSubscriptionDto dto, Guid paymentId, long paymentCode);
    Task<string> CreatePayOSUrlForBookingAsync(BuyBookingDto dto, Guid paymentId, long paymentCode);

    Task<PaymentLinkInformation> GetPaymentLinkInformationAsync(long paymentCode);
    Task<PaymentLinkInformation> CancelPaymentLinkAsync(long paymentCode, string? cancellationReason = null);

    Task<string> ConfirmWebhookAsync(string webhookUrl);
    Task<WebhookData> VerifyWebhookDataAsync(string webhookJson);

}