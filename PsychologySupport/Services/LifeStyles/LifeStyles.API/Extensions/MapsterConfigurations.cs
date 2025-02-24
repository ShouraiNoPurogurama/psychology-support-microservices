using Mapster;
using System.Reflection;

namespace LifeStyles.API.Extensions
{
    public static class MapsterConfigurations
    {
        public static void RegisterMapsterConfiguration(this IServiceCollection services)
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

            

        }
    }
}
