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
    public async Task<string> CreatePayOSUrlForSubscriptionAsync(BuySubscriptionDto dto, Guid paymentId, long paymentCode)
    {
        paymentValidatorService.ValidatePayOSMethod(dto.PaymentMethod);
        await paymentValidatorService.ValidateSubscriptionRequestAsync(dto);

        return await payOSLibrary.CreatePaymentLinkAsync(
            paymentCode, 
            (int)(dto.FinalPrice),
            dto.ServicePackageName,
            configuration["PayOS:ReturnUrl"]!,
            configuration["PayOS:CancelUrl"]!
        );
    }

    public async Task<string> CreatePayOSUrlForUpgradeSubscriptionAsync(UpgradeSubscriptionDto dto, Guid paymentId, long paymentCode)
    {
        paymentValidatorService.ValidatePayOSMethod(dto.PaymentMethod);
        await paymentValidatorService.ValidateSubscriptionRequestAsync(dto);

        return await payOSLibrary.CreatePaymentLinkAsync(
            paymentCode,
            (int)((dto.FinalPrice - dto.OldSubscriptionPrice)),
            dto.ServicePackageName,
            configuration["PayOS:ReturnUrl"]!,
            configuration["PayOS:CancelUrl"]!
        );
    }

    public async Task<string> CreatePayOSUrlForBookingAsync(BuyBookingDto dto, Guid paymentId, long paymentCode)
    {

        paymentValidatorService.ValidatePayOSMethod(dto.PaymentMethod);
        await paymentValidatorService.ValidateBookingRequestAsync(dto);


        return await payOSLibrary.CreatePaymentLinkAsync(
            paymentCode,
            (int)(dto.FinalPrice),
            dto.BookingCode,
            configuration["PayOS:ReturnUrl"]!,
            configuration["PayOS:CancelUrl"]!
        );
    }

    public async Task<string> CreatePayOSUrlForOrderAsync(OrderDto dto, Guid paymentId, long orderCode)
    {
        paymentValidatorService.ValidatePayOSMethod(dto.PaymentMethod);
        await paymentValidatorService.ValidateOrderRequestAsync(dto);

        return await payOSLibrary.CreatePaymentLinkAsync(
            orderCode,
            (int)(dto.FinalPrice),
            dto.PointPackageName,
            configuration["PayOS:ReturnUrl"]!,
            configuration["PayOS:CancelUrl"]!
        );
    }

    public Task<PaymentLinkInformation> GetPaymentLinkInformationAsync(long paymentCode)
        => payOSLibrary.GetPaymentLinkInformationAsync(paymentCode);

    public Task<PaymentLinkInformation> CancelPaymentLinkAsync(long paymentCode, string? cancellationReason = null)
        => payOSLibrary.CancelPaymentLinkAsync(paymentCode, cancellationReason);

    public Task<string> ConfirmWebhookAsync(string webhookUrl)
        => payOSLibrary.ConfirmWebhookAsync(webhookUrl);

    public Task<WebhookData> VerifyWebhookDataAsync(string webhookJson)
        => payOSLibrary.VerifyWebhookDataAsync(webhookJson);
}