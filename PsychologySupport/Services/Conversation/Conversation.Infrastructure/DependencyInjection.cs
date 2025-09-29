using BuildingBlocks.Data.Interceptors;
using BuildingBlocks.Extensions;
using BuildingBlocks.Idempotency;
using Conversation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Conversation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("ConversationDb");

        services.AddRedisCache(config);

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        // services.AddScoped<ILegacyPublicDbContext, LegacyPublicDbContext>();

        services.AddScoped<IIdempotencyHashAccessor, IdempotencyHashAccessor>();
        // services.AddScoped<IIdempotencyService, IdempotencyService>();           // store gốc (EF/Db)
        // services.Decorate<IIdempotencyService, LockingIdempotencyService>();     // single-flight
        // services.Decorate<IIdempotencyService, CachingIdempotencyService>();
        
        // services.AddScoped<IAliasVersionAccessor, AliasVersionAccessor>(); 
        // services.AddScoped<ICurrentActorAccessor, CurrentActorAccessor>();

        // services.AddScoped<IOutboxWriter, EfOutboxWriter>();
        
        

        services.AddDbContext<ConversationDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
            options.UseSnakeCaseNamingConvention();
            options.UseMongoDB(connectionString!);
        });
        

        return services;
    }
}