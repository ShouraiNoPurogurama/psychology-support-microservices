using Billing.Application.Data;
using Billing.Infrastructure.Data;
using Billing.Infrastructure.Resilience.Decorators;
using Billing.Infrastructure.Resilience.Services;
using BuildingBlocks.Data.Interceptors;
using BuildingBlocks.Extensions;
using BuildingBlocks.Idempotency;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Billing.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("BillingDb");

            services.AddRedisCache(config);

            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
            services.AddScoped<IBillingDbContext, BillingDbContext>();

            services.AddScoped<IIdempotencyHashAccessor, IdempotencyHashAccessor>();
            services.AddScoped<IIdempotencyService, IdempotencyService>();           // store gốc (EF/Db)
            services.Decorate<IIdempotencyService, LockingIdempotencyService>();     // single-flight
            services.Decorate<IIdempotencyService, CachingIdempotencyService>();



            services.AddDbContext<BillingDbContext>((serviceProvider, options) =>
            {
                options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
                options.UseSnakeCaseNamingConvention();
                options.UseNpgsql(connectionString);
            });

            return services;
        }
    }
}
