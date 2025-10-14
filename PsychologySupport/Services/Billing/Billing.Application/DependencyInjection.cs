using BuildingBlocks.Behaviors;
using BuildingBlocks.Messaging.MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Promotion.Grpc;
using System.Reflection;
using Translation.API.Protos;

namespace Billing.Application
{
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

            services.AddMessageBroker(configuration, Assembly.GetExecutingAssembly(), null, "billing");
            services.AddFeatureManagement();

            AddGrpcServiceDependencies(services, configuration);

            return services;
        }

        private static void AddGrpcServiceDependencies(IServiceCollection services, IConfiguration config)
        {
            services.AddGrpcClient<PromotionService.PromotionServiceClient>(options =>
                {
                    options.Address = new Uri(config["GrpcSettings:PromotionUrl"]!);
                })
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true
                });
            services.AddGrpcClient<TranslationService.TranslationServiceClient>(options =>
                {
                    options.Address = new Uri(config["GrpcSettings:TranslationUrl"]!);
                })
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true
                });
        }
    }
}