using System.Reflection;
using Mapster;

namespace Profile.API.Extensions;

public static class MapsterConfiguration
{
    public static void RegisterMapsterConfiguration(this IServiceCollection services)
    {
        // Scan the assembly for other mappings
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
    }
}