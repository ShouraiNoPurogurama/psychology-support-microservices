using BuildingBlocks.Exceptions.Handler;
using Carter;
using Microsoft.OpenApi.Models;

namespace Payment.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCarter();
        services.AddExceptionHandler<CustomExceptionHandler>();

        ConfigureCORS(services);
        ConfigureSwagger(services);
        
        return services;
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
    
    private static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Payment API",
                Version = "v1"
            });
            options.AddServer(new OpenApiServer
            {
                Url = "/payment-service/"
            });
        });
    }
    
    public static IConfigurationBuilder LoadConfiguration(this IConfigurationBuilder builder, IHostEnvironment env)
    {
        return builder
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    }
}