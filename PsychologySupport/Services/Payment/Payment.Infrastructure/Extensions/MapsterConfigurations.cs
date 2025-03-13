using System.Reflection;
using BuildingBlocks.Messaging.Events.Payment;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Payments.Dtos;

namespace Payment.Infrastructure.Extensions;

public static class MapsterConfigurations
{
    public static void RegisterMapsterConfiguration(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        
        TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);
    }
}