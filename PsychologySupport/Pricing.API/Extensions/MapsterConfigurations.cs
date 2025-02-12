using Mapster;
using Pricing.API.Dtos;
using Pricing.API.Models;
using System.Reflection;

namespace Pricing.API.Extensions
{
    public static class MapsterConfigurations
    {
        public static void RegisterMapsterConfiguration(this IServiceCollection services)
        {
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

            TypeAdapterConfig<AcademicLevelSalaryRatioDto, AcademicLevelSalaryRatio>.NewConfig()
                .IgnoreNullValues(true);
            TypeAdapterConfig<ExperiencePriceRangeDto, ExperiencePriceRange>.NewConfig()
               .IgnoreNullValues(true);
        }
    }
}
