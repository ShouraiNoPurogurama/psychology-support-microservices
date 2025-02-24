using Mapster;
using Subscription.API.Dtos;
using Subscription.API.Models;
using System.Reflection;

namespace Subscription.API.Extensions
{
    public static class MapsterConfigurations
    {
        public static void RegisterMapsterConfiguration(this IServiceCollection services)
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

            TypeAdapterConfig<ServicePackageDto, ServicePackage>.NewConfig()
                .IgnoreNullValues(true);

        }
    }
}