using BuildingBlocks.Exceptions.Handler;
using Carter;
using Microsoft.OpenApi.Models;

namespace Payment.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration,
        IWebHostEnvironment env)
    {
        var connectionString = configuration.GetConnectionString("PaymentDb")!;
        
        services.AddHealthChecks()
            .AddNpgSql(connectionString);
        
        services.AddCarter();
        services.AddExceptionHandler<CustomExceptionHandler>();

        services.AddAuthorization();
        services.AddHttpContextAccessor();

        ConfigureCORS(services);
        ConfigureSwagger(services, env);

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

    private static void ConfigureSwagger(IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Payment API",
                Version = "v1"
            });
            
            var url = env.IsProduction() 
                ? "/payment-service/swagger/v1/swagger.json" 
                : "https://localhost:5510/payment-service";
            
            options.AddServer(new OpenApiServer
            {
                Url = url
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.\n\nEnter: **Bearer &lt;your token&gt;**",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    Array.Empty<string>()
                }
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