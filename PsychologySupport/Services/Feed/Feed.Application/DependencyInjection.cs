using System.Reflection;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Messaging.MassTransit;
using Feed.Application.Abstractions.FanOut;
using Feed.Application.Abstractions.Redis;
using Feed.Application.Extensions;
using Feed.Application.Services;
using Feed.Application.Services.Decorators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace Feed.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        services.RegisterMapsterConfigurations();
        services.AddMessageBroker(configuration, Assembly.GetExecutingAssembly(), null, "feed");
        services.AddFeatureManagement();

        RegisterServiceDependency(services);

        return services;
    }

    private static void RegisterServiceDependency(IServiceCollection services)
    {
        services.AddScoped<IFeedFanOutService, FeedFanOutService>();
        
        // Apply decorators in order (innermost to outermost)
        // The order matters: KeyPrefix -> Retry -> Metrics -> Logging
        services.Decorate<ITrendingProvider, TrendingKeyPrefixDecorator>();
        services.Decorate<ITrendingProvider, TrendingRetryDecorator>();
        services.Decorate<ITrendingProvider, TrendingMetricsDecorator>();
        services.Decorate<ITrendingProvider, TrendingLoggingDecorator>();
    }
}
