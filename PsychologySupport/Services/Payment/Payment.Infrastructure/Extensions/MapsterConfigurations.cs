using System.Reflection;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Payments.Dtos;
using Payment.Domain.Models;

namespace Payment.Infrastructure.Extensions;

public static class MapsterConfigurations
{
    public static void RegisterMapsterConfiguration(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

        TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);

       TypeAdapterConfig<Domain.Models.Payment, PaymentDto>
        .NewConfig()
        .Map(dest => dest.Id, src => src.Id)
        .Map(dest => dest.PatientProfileId, src => src.PatientProfileId)
        .Map(dest => dest.TotalAmount, src => src.TotalAmount)
        .Map(dest => dest.SubscriptionId, src => src.SubscriptionId)
        .Map(dest => dest.BookingId, src => src.BookingId)
        .Map(dest => dest.Status, src => src.Status)
        .Map(dest => dest.PaymentType, src => src.PaymentType)
        .Map(dest => dest.PaymentMethodId, src => src.PaymentMethodId)
        .Map(dest => dest.ExternalTransactionCode, 
             src => src.PaymentDetails.FirstOrDefault() != null 
                 ? src.PaymentDetails.FirstOrDefault().ExternalTransactionCode 
                 : null); 



    }
}