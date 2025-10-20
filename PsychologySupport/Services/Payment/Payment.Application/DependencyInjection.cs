using System.Reflection;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Messaging.MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Pii.API.Protos;

namespace Payment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });
        
        services.AddMessageBroker(configuration, Assembly.GetExecutingAssembly(), null, "payment");
        services.AddFeatureManagement();
        services.AddHttpContextAccessor();

        AddGrpcServiceDependencies(services, configuration);

        return services;
    }

    private static void AddGrpcServiceDependencies(IServiceCollection services, IConfiguration config)
    {
        services.AddGrpcClient<PiiService.PiiServiceClient>(options =>
        {
            options.Address = new Uri(config["GrpcSettings:PiiUrl"]!);
        })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                return handler;
            });
    }
}