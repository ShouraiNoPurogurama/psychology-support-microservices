using System.Text;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Payment.Application.Payments.Dtos;
using Payment.Application.ServiceContracts;
using Payment.Application.Utils;

namespace Payment.Infrastructure.Services;

public class VNPayService(
    IPaymentValidatorService paymentValidatorService, 
    IHttpContextAccessor contextAccessor,
    IConfiguration configuration
    ) : IVnPayService
{
    public async Task<string> CreateVNPayUrlForSubscriptionAsync(BuySubscriptionDto dto, Guid paymentId)
    {
        paymentValidatorService.ValidateVNPayMethod(dto.PaymentMethod);
        await paymentValidatorService.ValidateSubscriptionRequestAsync(dto);
        
        var orderInfo = new StringBuilder();
        orderInfo.Append($"{nameof(Domain.Models.Payment.Id)}={HttpUtility.UrlEncode(paymentId.ToString())}&");
        orderInfo.Append($"{nameof(BuySubscriptionDto.PatientEmail)}={HttpUtility.UrlEncode(dto.PatientEmail)}&");
        orderInfo.Append($"{nameof(BuySubscriptionDto.PaymentType)}={HttpUtility.UrlEncode(dto.PaymentType.ToString())}&");
        orderInfo.Append($"{nameof(BuySubscriptionDto.PromoCode)}={HttpUtility.UrlEncode(dto.PromoCode ?? "")}&");
        orderInfo.Append($"{nameof(BuySubscriptionDto.GiftId)}={HttpUtility.UrlEncode(dto.GiftId?.ToString() ?? "")}&");
        orderInfo.Append($"{nameof(BuySubscriptionDto.DurationDays)}={HttpUtility.UrlEncode(dto.DurationDays.ToString())}");
        
        if (orderInfo[^1] == '&')
        {
            orderInfo.Length--;
        }
        
        return await GenerateVNPayUrl(dto.FinalPrice, orderInfo.ToString());
    }

    public async Task<string> CreateVNPayUrlForBookingAsync(BuyBookingDto dto, Guid paymentId)
    {
        paymentValidatorService.ValidateVNPayMethod(dto.PaymentMethod);
        await paymentValidatorService.ValidateBookingRequestAsync(dto);
        
        var orderInfo = new StringBuilder();
        orderInfo.Append($"{nameof(Domain.Models.Payment.Id)}={HttpUtility.UrlEncode(paymentId.ToString())}&");
        orderInfo.Append($"{nameof(BuyBookingDto.BookingId)}={HttpUtility.UrlEncode(dto.BookingId.ToString())}&");
        orderInfo.Append($"{nameof(BuyBookingDto.PatientEmail)}={HttpUtility.UrlEncode(dto.PatientEmail)}&");
        orderInfo.Append($"{nameof(BuyBookingDto.PaymentType)}={HttpUtility.UrlEncode(dto.PaymentType.ToString())}&");
        orderInfo.Append($"{nameof(BuyBookingDto.PromoCode)}={HttpUtility.UrlEncode(dto.PromoCode ?? "")}&");
        orderInfo.Append($"{nameof(BuyBookingDto.GiftCodeId)}={HttpUtility.UrlEncode(dto.GiftCodeId?.ToString() ?? "")}&");
        
        
        if (orderInfo[^1] == '&')
        {
            orderInfo.Length--;
        }
        
        return await GenerateVNPayUrl(dto.FinalPrice, orderInfo.ToString());
    }

    public async Task<bool> ValidateSignatureAsync(string queryString, string signature)
    {
        var vnPayLibrary = new VnPayLibrary();
        foreach (var param in queryString.TrimStart('?').Split('&'))
        {
            var keyValue = param.Split('=');
            if (keyValue.Length == 2)
            {
                vnPayLibrary.AddResponseData(keyValue[0], keyValue[1]);
            }
        }
        
        return await Task.FromResult(vnPayLibrary.ValidateSignature(configuration["Vnpay:HashSecret"]!, signature));
    }
    
    public async Task<string> GenerateVNPayUrl(decimal amount, string orderInfo)
    {
        HttpContext context = contextAccessor.HttpContext!;
        var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(configuration["TimeZoneId"]!);
        var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
        var tick = DateTime.Now.Ticks.ToString();
        var vnPayLibrary = new VnPayLibrary();
        var urlCallBack = $"{configuration["VnPay:CallbackUrl"]}";

        int multipliedPrice = (int)(amount * 100);
        string multipliedPriceString = multipliedPrice.ToString();

        vnPayLibrary.AddRequestData("vnp_Version", configuration["Vnpay:Version"]!);
        vnPayLibrary.AddRequestData("vnp_Command", configuration["Vnpay:Command"]!);
        vnPayLibrary.AddRequestData("vnp_TmnCode", configuration["Vnpay:TmnCode"]!);
        vnPayLibrary.AddRequestData("vnp_Amount", multipliedPriceString);
        vnPayLibrary.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
        vnPayLibrary.AddRequestData("vnp_CurrCode", configuration["Vnpay:CurrCode"]!);
        vnPayLibrary.AddRequestData("vnp_IpAddr", vnPayLibrary.GetIpAddress(context));
        vnPayLibrary.AddRequestData("vnp_Locale", configuration["Vnpay:Locale"]!);
        vnPayLibrary.AddRequestData("vnp_OrderInfo", orderInfo);
        vnPayLibrary.AddRequestData("vnp_OrderType", "VNPay");
        vnPayLibrary.AddRequestData("vnp_ReturnUrl", urlCallBack);
        vnPayLibrary.AddRequestData("vnp_TxnRef", tick);

        return vnPayLibrary.CreateRequestUrl(configuration["Vnpay:BaseUrl"]!, configuration["Vnpay:HashSecret"]!);
    }
}