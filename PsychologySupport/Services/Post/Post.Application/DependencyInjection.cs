using System.Reflection;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Messaging.MassTransit;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Post.Application.Extensions;
using Post.Application.Services;

namespace Post.Application;

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
        services.AddMessageBroker(configuration, Assembly.GetExecutingAssembly(),
            (context, configurator) =>
            {
                configurator.ReceiveEndpoint("post-service-events-queue", e =>
                {
                    e.ConfigureConsumers(context);
                });
            });
        services.AddFeatureManagement();

        services.AddServiceDependencies(configuration);
        services.AddConfigurationSettings(configuration);
        
        return services;
    }
    
    public static IServiceCollection AddServiceDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRateLimitingService, MemoryRateLimitingService>();
        services.AddScoped<ICounterSynchronizationService, CounterSynchronizationService>();
        
        services.AddHostedService<PostAbandonmentBackgroundService>();

        return services;
    }
    
    public static IServiceCollection AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimitOptions>(configuration.GetSection(RateLimitOptions.SectionName));
        services.Configure<PostAbandonmentOptions>(configuration.GetSection(PostAbandonmentOptions.SectionName));
        
        return services;
    }
}