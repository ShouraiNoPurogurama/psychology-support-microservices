using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Test.Application.Data;
using Test.Infrastructure.Data;
using BuildingBlocks.Data.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Test.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("TestDb");

            services.AddDbContext<TestDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(connectionString);
                options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
            });

            services.AddScoped<ITestDbContext, TestDbContext>();

            // Đăng ký Interceptors
            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

            return services;
        }
    }
}
