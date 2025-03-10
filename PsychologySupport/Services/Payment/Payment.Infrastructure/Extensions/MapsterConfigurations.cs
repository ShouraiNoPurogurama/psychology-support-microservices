using System.Reflection;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Payment.Infrastructure.Extensions;

public static class MapsterConfigurations
{
    public static void RegisterMapsterConfiguration(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        
        TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);
    }
}