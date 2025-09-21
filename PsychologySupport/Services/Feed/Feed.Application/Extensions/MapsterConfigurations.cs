using System.Reflection;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Feed.Application.Extensions;

public static class MapsterConfigurations
{
    public static void RegisterMapsterConfigurations(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        
    }
    
}