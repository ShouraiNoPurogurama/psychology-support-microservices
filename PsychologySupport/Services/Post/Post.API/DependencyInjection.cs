using BuildingBlocks.Exceptions.Handler;
using Carter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Post.API.Extensions;

namespace Post.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration config,
        IWebHostEnvironment env)
    {
        services.AddCarter();
        
        services.AddEndpointsApiExplorer();
        
        services.AddExceptionHandler<CustomExceptionHandler>();
        
        services.AddHealthChecks()
            .AddNpgSql(config.GetConnectionString("PostDb")!);

        services.AddHttpContextAccessor();

        services.AddAuthorization();
        
        services.AddIdentityServices(config);
        
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
                Title = "Post API",
                Version = "v1"
            });

            var url = env.IsProduction() 
                ? "/post-service" 
                : "https://localhost:5510/post-service";
            
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

    public static WebApplication UseApiServices(this WebApplication app)
    {
        app.UseExceptionHandler(options => { });
        app.MapCarter();
        
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseCors("CorsPolicy");

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseHealthChecks("/health",
            new HealthCheckOptions()
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            }
        );
        
        return app;
    }
}