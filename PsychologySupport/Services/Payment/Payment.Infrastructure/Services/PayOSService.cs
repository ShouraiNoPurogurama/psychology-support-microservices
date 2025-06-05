using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Payment.Application.Payments.Dtos;
using Payment.Application.ServiceContracts;
using Payment.Application.Utils;

namespace Payment.Infrastructure.Services;

public class PayOSService(
    IPaymentValidatorService paymentValidatorService,
    IHttpContextAccessor contextAccessor,
    IConfiguration configuration,
    PayOSLibrary payOSLibrary
) : IPayOSService
{
    public async Task<string> CreatePayOSUrlForSubscriptionAsync(BuySubscriptionDto dto, Guid paymentId)
    {
        // await paymentValidatorService.ValidateSubscriptionRequestAsync(dto);

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
        // await paymentValidatorService.ValidateSubscriptionRequestAsync(dto);

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
        // await paymentValidatorService.ValidateBookingRequestAsync(dto);

        var orderInfo = BuildOrderInfo(dto, paymentId);

        return await payOSLibrary.CreatePaymentLinkAsync(
            GuidToLong(paymentId),
            (int)(dto.FinalPrice * 100),
            $"Booking Payment: {orderInfo}",
            configuration["PayOS:ReturnUrl"]!,
            configuration["PayOS:CancelUrl"]!
        );
    }

    //public bool ValidateSignature(string data, string signature)
    //{
    //    return payOSLibrary.VerifySignature(data, signature);
    //}

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

    // Convert Guid to long 
    private static long GuidToLong(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return BitConverter.ToInt64(bytes, 0);
    }
}