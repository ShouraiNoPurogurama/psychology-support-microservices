using System.Reflection;
using Mapster;

namespace Media.API.Extensions
{
    public static class MapsterConfigurations
    {
        public static void RegisterMapsterConfiguration(this IServiceCollection services)
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

            //TypeAdapterConfig<ServicePackageDto, ServicePackage>.NewConfig()
            //    .IgnoreNullValues(true);

        }
    }
}
