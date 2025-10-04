using System.Reflection;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Messaging.MassTransit;
using Feed.Application.Abstractions.FanOut;
using Feed.Application.Extensions;
using Feed.Application.Services;
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
    }
}
