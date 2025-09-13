using BuildingBlocks.Data.Interceptors;
using BuildingBlocks.Extensions;
using BuildingBlocks.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Infrastructure.Authentication;
using Post.Infrastructure.Data;
using Post.Infrastructure.Data.Public;
using Post.Infrastructure.Data.Query;
using Post.Infrastructure.Resilience.Decorators;
using Post.Infrastructure.Resilience.Services;

namespace Post.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("PostDb");

        services.AddRedisCache(config);

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        services.AddScoped<IPublicDbContext, PublicDbContext>();
        services.AddScoped<IQueryDbContext, QueryDbContext>();

        services.AddScoped<IIdempotencyHashAccessor, IdempotencyHashAccessor>();
        services.AddScoped<IIdempotencyService, IdempotencyService>();           // store gốc (EF/Db)
        services.Decorate<IIdempotencyService, LockingIdempotencyService>();     // single-flight
        services.Decorate<IIdempotencyService, CachingIdempotencyService>();
        
        services.AddScoped<IAliasContextResolver, CurrentAliasContextResolver>(); 
        services.AddScoped<IActorResolver, CurrentAliasContextResolver>();
            
        
        services.AddDbContext<PublicDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
            options.UseSnakeCaseNamingConvention();
            options.UseNpgsql(connectionString);
        });

        services.AddDbContext<QueryDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
            options.UseSnakeCaseNamingConvention();
            options.UseNpgsql(connectionString);
        });

        return services;
    }
}