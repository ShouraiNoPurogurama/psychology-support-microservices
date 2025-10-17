using BuildingBlocks.Idempotency;
using Feed.Application.Abstractions.CursorService;
using Feed.Application.Abstractions.FeedConfiguration;
using Feed.Application.Abstractions.FollowerTracking;
using Feed.Application.Abstractions.PostModeration;
using Feed.Application.Abstractions.PostRepository;
using Feed.Application.Abstractions.RankingService;
using Feed.Application.Abstractions.Redis;
using Feed.Application.Abstractions.Resilience;
using Feed.Application.Abstractions.UserActivity;
using Feed.Application.Abstractions.UserFeed;
using Feed.Application.Abstractions.UserPinning;
using Feed.Application.Abstractions.ViewerBlocking;
using Feed.Application.Abstractions.ViewerFollowing;
using Feed.Application.Abstractions.ViewerMuting;
using Feed.Application.Abstractions.VipService;
using Feed.Infrastructure.BackgroundJobs;
using Feed.Infrastructure.Data.Redis;
using Feed.Infrastructure.Data.Redis.Decorators;
using Feed.Infrastructure.Data.Redis.Providers;
using Feed.Infrastructure.Data.Repository;
using Feed.Infrastructure.Persistence.Cassandra;
using Feed.Infrastructure.Persistence.Cassandra.Repositories;
using Feed.Infrastructure.Resilience.Decorators;
using Feed.Infrastructure.Resilience.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using StackExchange.Redis;

namespace Feed.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        // Register Redis providers with Scrutor decorators
        services.AddRedisProviders();
        
        // Register Cassandra services
        services.AddCassandraServices();

        AddRedisCache(services, config);
        
        // Register application services
        AddServiceDependencies(services);
        
        services.AddQuartz(q =>
        {
            var jobKey = new JobKey(UpdateGlobalFallbackJobConfiguration.JobName);
            q.AddJob<UpdateGlobalFallbackJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity($"{UpdateGlobalFallbackJobConfiguration.JobName}-trigger")
                .StartNow()
                // .WithCronSchedule(UpdateGlobalFallbackJobConfiguration.CronExpression));
                .WithSimpleSchedule(s => s
                    .WithIntervalInSeconds(30)
                    .RepeatForever())); 
        });
        
        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
        
        return services;
    }
    
    public static IServiceCollection AddRedisProviders(this IServiceCollection services)
    {
        // Register IDatabase from Redis connection
        services.AddSingleton<IDatabase>(provider =>
        {
            var multiplexer = provider.GetRequiredService<IConnectionMultiplexer>();
            return multiplexer.GetDatabase();
        });
        
        // Base implementations
        services.AddScoped<ITrendingProvider, TrendingRedisProvider>();
        
        services.AddScoped<IIdempotencyService, CassandraIdempotencyService>();   // store gốc (Db)
        services.Decorate<IIdempotencyService, LockingIdempotencyService>();     // single-flight
        services.Decorate<IIdempotencyService, CachingIdempotencyService>();
        
        // Apply decorators in order (innermost to outermost)
        // The order matters: KeyPrefix -> Retry -> Metrics -> Logging
        services.Decorate<ITrendingProvider, TrendingKeyPrefixDecorator>();
        services.Decorate<ITrendingProvider, TrendingRetryDecorator>();
        services.Decorate<ITrendingProvider, TrendingMetricsDecorator>();
        services.Decorate<ITrendingProvider, TrendingLoggingDecorator>();
        
        return services;
    }
    
    public static IServiceCollection AddCassandraServices(this IServiceCollection services)
    {
        services.AddSingleton<IPreparedStatementRegistry, PreparedStatementRegistry>();
        
        return services;
    }
    
    private static void AddServiceDependencies(IServiceCollection services)
    {
        services.AddScoped<IVipService, VipService>();
        services.AddScoped<IRankingService, RankingService>();
        services.AddScoped<ICursorService, CursorService>();
        services.AddScoped<IIdempotencyHashAccessor, IdempotencyHashAccessor>();
        services.AddScoped<IIdempotencyService, CassandraIdempotencyService>();

        //Add repositories
        services.AddScoped<IUserFeedRepository, UserFeedRepository>();
        services.AddScoped<IFeedConfigRepository, FeedConfigRepository>();
        services.AddScoped<IFollowerTrackingRepository, FollowerTrackingRepository>();
        services.AddScoped<IPostModerationRepository, PostModerationRepository>();

        services.AddScoped<IRankingService, RankingService>();
        services.AddScoped<IUserActivityRepository, UserActivityRepository>();
        services.AddScoped<IUserFeedRepository, UserFeedRepository>();
        services.AddScoped<IUserPinningRepository, UserPinningRepository>();
        services.AddScoped<IViewerBlockingRepository, ViewerBlockingRepository>();
        services.AddScoped<IViewerFollowingRepository, ViewerFollowingRepository>();
        services.AddScoped<IViewerMutingRepository, ViewerMutingRepository>();
        services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
        
        // Register post replica repository (Cassandra) and post read repository (wrapper)
        services.AddScoped<IPostReplicaRepository, PostReplicaRepository>();
        services.AddScoped<IPostReadRepository, PostReadRepository>();
    }
    
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration config)
    {
        // Đăng ký IConnectionMultiplexer như một singleton sử dụng factory
        // để tránh block luồng khởi động
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConnectionString = config.GetConnectionString("Redis");
            if (string.IsNullOrEmpty(redisConnectionString))
            {
                throw new InvalidOperationException("Redis connection string is not configured.");
            }

            var options = ConfigurationOptions.Parse(redisConnectionString);
        
            options.AbortOnConnectFail = false;

            return ConnectionMultiplexer.Connect(options);
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.ConnectionMultiplexerFactory = async () =>
            {
                var sp = services.BuildServiceProvider();
                return await Task.FromResult(sp.GetRequiredService<IConnectionMultiplexer>());
            };
        });

        return services;
    }
}
