using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Wallet.Application.Extensions
{
    public static class MapsterConfigurations
    {
        public static void RegisterMapsterConfiguration(this IServiceCollection services)
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

            //TypeAdapterConfig<TestResult, TestResultDto>
            //    .NewConfig();


        }
    }
}
