using System.Reflection;
using Mapster;
using Profile.API.Dtos;

namespace Profile.API.Extensions;

public static class MapsterConfiguration
{
    public static void RegisterMapsterConfiguration(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        
    }
}