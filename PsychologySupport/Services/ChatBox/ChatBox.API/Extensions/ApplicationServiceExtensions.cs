using System.Reflection;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Data.Interceptors;
using BuildingBlocks.Messaging.MassTransit;
using Carter;
using ChatBox.API.Data;
using ChatBox.API.Models;
using ChatBox.API.Services;
using ChatBox.API.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;

namespace ChatBox.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers();
        
        // services.AddCarter();

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
                Title = "Chatbox API",
                Version = "v1"
            });
            options.AddServer(new OpenApiServer
            {
                Url = "/chatbox-service/"
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
        services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
        services.AddScoped<GeminiService>();
        services.AddScoped<SessionService>();
        services.AddScoped<SummarizationService>();
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("ChatBoxDb");

        services.AddDbContext<ChatBoxDbContext>((sp, opt) =>
        {
            opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            opt.UseNpgsql(connectionString);
        });

        services.AddScoped<DbContext, ChatBoxDbContext>();
    }
    
    private static void ConfigureGemini(IServiceCollection services, IConfiguration config)
    {
        services.Configure<GeminiConfig>(config.GetSection("GeminiConfig"));
    }
}