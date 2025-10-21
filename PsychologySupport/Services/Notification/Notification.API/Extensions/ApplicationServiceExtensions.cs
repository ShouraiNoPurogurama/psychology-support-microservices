using BuildingBlocks.Behaviors;
using BuildingBlocks.Data.Interceptors;
using BuildingBlocks.Messaging.MassTransit;
using Carter;
using FirebaseAdmin;
using FluentValidation;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Notification.API.Features.Firebase.Contracts;
using Notification.API.Infrastructure.Outbox;
using Notification.API.Features.Firebase.Services;
using Notification.API.Infrastructure.Repositories;
using Notification.API.Shared.Contracts;

namespace Notification.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {
        services.Configure<AppSettings>(config);

        services.AddHealthChecks().AddNpgSql(sp =>
            sp.GetRequiredService<IOptions<AppSettings>>().Value.ServiceDbContext.NotificationDb);

        services.AddCarter();

        ConfigureSwagger(services, env);

        ConfigureCORS(services);

        ConfigureMediatR(services);

        AddServiceDependencies(services);

        AddNotificationServices(services);
        
        services.AddIdentityServices(config);
        
        services.AddAuthorization();

        services.AddHttpContextAccessor();

        services.AddMessageBroker(config, typeof(IAssemblyMarker).Assembly, null, "notification");
        
        ConfigureGrpc(services);

        return services;
    }

    private static void AddNotificationServices(IServiceCollection services)
    {
        // Register repositories
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationPreferencesRepository, NotificationPreferencesRepository>();
        services.AddScoped<IProcessedEventRepository, ProcessedEventRepository>();

        // Register cache
        services.AddMemoryCache();
        services.AddScoped<IPreferencesCache, PreferencesCache>();

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(typeof(IAssemblyMarker).Assembly);
    }

    private static void ConfigureFirebase(IServiceCollection services)
    {
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile("firebase_notification_key.json")
        });
    }

    private static void ConfigureSwagger(IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Notification API",
                Version = "v1"
            });
            
            var url = env.IsProduction() 
                ? "/notification-service" 
                : "https://localhost:5510/notification-service";
            
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
            configuration.RegisterServicesFromAssembly(typeof(IAssemblyMarker).Assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });
    }

    private static void AddServiceDependencies(IServiceCollection services)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        // services.AddScoped<OutboxProcessor>();
        services.AddScoped<OutboxListener>();
        services.AddScoped<OutboxService>();
        // services.AddHostedService<OutboxProcessor>();
        services.AddHostedService<OutboxListener>();
        services.AddSingleton<IFirebaseService, FirebaseService>();
    }

    private static void ConfigureGrpc(IServiceCollection services)
    {
        services.AddGrpc();
        services.AddGrpcReflection();
    }
}
