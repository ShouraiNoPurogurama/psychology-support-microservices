using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Test.Application.Dtos;
using Test.Domain.Models;

namespace Test.Application.Extensions
{
    public static class MapsterConfigurations
    {
        public static void RegisterMapsterConfiguration(this IServiceCollection services)
        {
            // Scan the assembly for other mappings
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

            TypeAdapterConfig<TestResult, TestResultDto>
                .NewConfig();
        }
    }
}
