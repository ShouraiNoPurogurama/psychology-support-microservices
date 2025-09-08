using BuildingBlocks.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Post.Application.Data;
using Post.Infrastructure.Data;
using Post.Infrastructure.Data.Public;
using Post.Infrastructure.Data.Query;

namespace Post.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("PostDb");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        services.AddScoped<IPublicDbContext, PublicDbContext>();
        services.AddScoped<IQueryDbContext, QueryDbContext>();

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