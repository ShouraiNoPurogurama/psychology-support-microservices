using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Payment.Application.Payments.Dtos;
using Payment.Application.ServiceContracts;
using Payment.Application.Utils;
using Net.payOS.Types;

namespace Payment.Infrastructure.Services;

public class PayOSService(
    IPaymentValidatorService paymentValidatorService,
    IConfiguration configuration,
    PayOSLibrary payOSLibrary
) : IPayOSService
{
    public async Task<string> CreatePayOSUrlForSubscriptionAsync(BuySubscriptionDto dto, Guid paymentId)
    {
        var orderInfo = BuildOrderInfo(dto, paymentId);

        return await payOSLibrary.CreatePaymentLinkAsync(
            GuidToLong(paymentId),
            (int)(dto.FinalPrice * 100),
            $"Subscription Payment: {orderInfo}",
            configuration["PayOS:ReturnUrl"]!,
            configuration["PayOS:CancelUrl"]!
        );
    }

    public async Task<string> CreatePayOSUrlForUpgradeSubscriptionAsync(UpgradeSubscriptionDto dto, Guid paymentId)
    {
        var orderInfo = BuildOrderInfo(dto, paymentId);

        return await payOSLibrary.CreatePaymentLinkAsync(
            GuidToLong(paymentId),
            (int)((dto.FinalPrice - dto.OldSubscriptionPrice) * 100),
            $"Upgrade Subscription: {orderInfo}",
            configuration["PayOS:ReturnUrl"]!,
            configuration["PayOS:CancelUrl"]!
        );
    }

    public async Task<string> CreatePayOSUrlForBookingAsync(BuyBookingDto dto, Guid paymentId)
    {
        var orderInfo = BuildOrderInfo(dto, paymentId);

        return await payOSLibrary.CreatePaymentLinkAsync(
            GuidToLong(paymentId),
            (int)(dto.FinalPrice * 100),
            $"Booking Payment: {orderInfo}",
            configuration["PayOS:ReturnUrl"]!,
            configuration["PayOS:CancelUrl"]!
        );
    }

    public Task<PaymentLinkInformation> GetPaymentLinkInformationAsync(long orderCode)
        => payOSLibrary.GetPaymentLinkInformationAsync(orderCode);

    public Task<PaymentLinkInformation> CancelPaymentLinkAsync(long orderCode, string? cancellationReason = null)
        => payOSLibrary.CancelPaymentLinkAsync(orderCode, cancellationReason);

    public Task<string> ConfirmWebhookAsync(string webhookUrl)
        => payOSLibrary.ConfirmWebhookAsync(webhookUrl);

    public Task<WebhookData> VerifyWebhookDataAsync(string webhookJson)
        => payOSLibrary.VerifyWebhookDataAsync(webhookJson);

    private static string BuildOrderInfo(object dto, Guid paymentId)
    {
        var sb = new StringBuilder();
        sb.Append($"PaymentId={paymentId}&");
        foreach (var prop in dto.GetType().GetProperties())
        {
            var value = prop.GetValue(dto);
            if (value != null)
            {
                sb.Append($"{prop.Name}={value}&");
            }
        }
        if (sb.Length > 0 && sb[^1] == '&')
            sb.Length--;
        return sb.ToString();
    }

    private static long GuidToLong(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return BitConverter.ToInt64(bytes, 0);
    }
}