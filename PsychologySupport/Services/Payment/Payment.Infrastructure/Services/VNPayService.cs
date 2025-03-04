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
    ) : IVNPayService
{
    public async Task<string> CreateVNPayUrlForSubscriptionAsync(BuySubscriptionDto dto)
    {
        paymentValidatorService.ValidateVNPayMethod(dto.PaymentMethod);
        await paymentValidatorService.ValidateSubscriptionRequestAsync(dto);
        
        var orderInfo = new StringBuilder();
        orderInfo.Append($"UserId={HttpUtility.UrlEncode(dto.PatientId.ToString())}&");
        orderInfo.Append($"PaymentMethod={dto.PaymentMethod}&");
        orderInfo.Append($"ServicePackageId={HttpUtility.UrlEncode(dto.ServicePackageId.ToString())}&");
        orderInfo.Append($"DurationDays={HttpUtility.UrlEncode(dto.DurationDays.ToString())}");
        
        if (orderInfo[^1] == '&')
        {
            orderInfo.Length--;
        }
        
        return await GenerateVNPayUrl(dto.TotalAmount, orderInfo.ToString());
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