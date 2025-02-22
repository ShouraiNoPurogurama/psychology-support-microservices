using BuildingBlocks.Behaviors;
using BuildingBlocks.Data.Interceptors;
using Carter;
using MassTransit;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;

namespace Notification.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<AppSettings>(config);
        
        services.AddCarter();
        
        ConfigureSwagger(services);

        ConfigureCORS(services);
        
        ConfigureMediatR(services);
        
        AddServiceDependencies(services);

        return services;
    }

    private static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Notification API",
            Version = "v1"
        }));
    }

    private static void ConfigureCORS(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    }

    private static void ConfigureMediatR(IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });
    }

    private static void AddServiceDependencies(IServiceCollection services)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
    }
    
    private static IServiceCollection AddMessageBroker(IServiceCollection services, IConfiguration config)
    {
        var appSettings = config.Get<AppSettings>();

        var assemblyMarker = typeof(IAssemblyMarker).Assembly;

        services.AddMassTransit(configure =>
        {
            if (appSettings is null)
            {
                throw new ArgumentException(nameof(appSettings));
            }

            var brokerConfig = appSettings.BrokerConfiguration;

            #region Configure attributes

            configure.SetKebabCaseEndpointNameFormatter();
            configure.SetInMemorySagaRepositoryProvider();
            configure.AddConsumers(assemblyMarker);
            configure.AddSagaStateMachines(assemblyMarker);
            configure.AddSagas(assemblyMarker);
            configure.AddActivities(assemblyMarker);

            #endregion

            configure.UsingRabbitMq((context, configurator) =>
            {
                configurator.UseRawJsonDeserializer();

                configurator.Host(brokerConfig.Host, hostConfigure =>
                {
                    hostConfigure.Username(brokerConfig.Username);
                    hostConfigure.Password(brokerConfig.Password);
                });
                configurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}