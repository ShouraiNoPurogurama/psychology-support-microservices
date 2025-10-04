using BuildingBlocks.Extensions;
using BuildingBlocks.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Infrastructure.Authentication;
using Post.Infrastructure.Data.Interceptors;
using Post.Infrastructure.Data.Post;
using Post.Infrastructure.Data.Processors;
using Post.Infrastructure.Data.Query;
using Post.Infrastructure.Integration.Services;
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
        // services.AddScoped<ILegacyPublicDbContext, LegacyPublicDbContext>();
        services.AddScoped<IQueryDbContext, QueryDbContext>();
        // services.AddScoped<IPostDbContext, PostDbContext>();

        // Dòng này nói rằng: "Khi ai đó hỏi IPostDbContext, hãy đi tìm instance PostDbContext
        // đã được tạo trong scope này và trả về nó".// Điều này đảm bảo cả hai resolve về cùng một instance.
        services.AddScoped<IPostDbContext>(sp => sp.GetRequiredService<PostDbContext>());

        services.AddScoped<IIdempotencyHashAccessor, IdempotencyHashAccessor>();
        services.AddScoped<IIdempotencyService, IdempotencyService>(); // store gốc (EF/Db)
        services.Decorate<IIdempotencyService, LockingIdempotencyService>(); // single-flight
        services.Decorate<IIdempotencyService, CachingIdempotencyService>();

        services.AddScoped<IAliasVersionAccessor, AliasVersionAccessor>();
        services.AddScoped<ICurrentActorAccessor, CurrentActorAccessor>();

        services.AddScoped<IOutboxWriter, EfOutboxWriter>();
        services.AddScoped<IFollowerCountProvider, FollowerCountProvider>();

        // Register OutboxProcessor as a background service
        services.AddHostedService<OutboxProcessor>();


        // services.AddDbContext<LegacyPublicDbContext>((serviceProvider, options) =>
        // {
        //     options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
        //     options.UseSnakeCaseNamingConvention();
        //     options.UseNpgsql(connectionString);
        // });
        //
        services.AddDbContext<PostDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
            options.UseSnakeCaseNamingConvention();
            options.UseNpgsql(connectionString, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "post"));
        });

        services.AddDbContext<QueryDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
            options.UseSnakeCaseNamingConvention();
            options.UseNpgsql(connectionString, x =>
            {
                x.MigrationsHistoryTable("__EFMigrationsHistory", "query"); // thêm schema
            });
        });


        return services;
    }
}