using System.Reflection;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Data.Interceptors;
using BuildingBlocks.Messaging.MassTransit;
using Carter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;
using Translation.API.Data;
using Translation.API.Models;
using Translation.API.Services;

namespace Translation.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers();
        
        services.AddCarter();

        ConfigureSwagger(services);

        ConfigureCors(services);
        
        AddDatabase(services, config);

        AddServiceDependencies(services);
        
        ConfigureGemini(services, config);

        ConfigureMediatR(services);
        
        services.AddIdentityServices(config);
        
        services.AddSignalR();
        
        services.AddMessageBroker(config, typeof(IAssemblyMarker).Assembly);
        
        return services;
    }

    private static void ConfigureMediatR(IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });
    }

    private static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Translation API",
                Version = "v1"
            });
            options.AddServer(new OpenApiServer
            {
                Url = "/translation-service/"
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

    private static void ConfigureCors(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {

                builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials(); // Enable credentials support
            });
        });
    }

    private static void AddServiceDependencies(IServiceCollection services)
    {
        services.AddScoped<GeminiService>();
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("TranslationDb");
    
        services.AddDbContext<TranslationDbContext>((sp, opt) =>
        {
            opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            opt.UseNpgsql(connectionString);
        });
    
        services.AddScoped<DbContext, TranslationDbContext>();
    }
    
    private static void ConfigureGemini(IServiceCollection services, IConfiguration config)
    {
        services.Configure<GeminiConfig>(config.GetSection("GeminiConfig"));
    }
}