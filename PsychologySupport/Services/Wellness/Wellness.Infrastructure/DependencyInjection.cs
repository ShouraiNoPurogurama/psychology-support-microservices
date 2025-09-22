using BuildingBlocks.Data.Interceptors;
using BuildingBlocks.Idempotency;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wellness.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;

namespace Wellness.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("WellnessDb");


            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
            services.AddScoped<IWellnessDbContext, WellnessDbContext>();
            services.AddScoped<IIdempotencyHashAccessor, IdempotencyHashAccessor>();

            //services.AddScoped<IIdempotencyService, IdempotencyService>();           // store gốc (EF/Db)
            //services.Decorate<IIdempotencyService, LockingIdempotencyService>();     // single-flight
            //services.Decorate<IIdempotencyService, CachingIdempotencyService>();



            services.AddDbContext<WellnessDbContext>((serviceProvider, options) =>
            {
                options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
                options.UseSnakeCaseNamingConvention();
                options.UseNpgsql(connectionString);
            });

            return services;
        }
    }
}
