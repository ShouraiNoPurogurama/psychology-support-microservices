using BuildingBlocks.Behaviors;
using BuildingBlocks.Data.Interceptors;
using BuildingBlocks.Messaging.Masstransit;
using Carter;
using LifeStyles.API.Abstractions;
using LifeStyles.API.Data;
using LifeStyles.API.Data.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

namespace LifeStyles.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddCarter();

        ConfigureSwagger(services);

        ConfigureCors(services);

        ConfigureMediatR(services);

        AddDatabase(services, config);

        AddServiceDependencies(services);
        
        // AddRedisCache(services, config);

        services.AddMessageBroker(config, typeof(IAssemblyMarker).Assembly);

        return services;
    }

    private static void AddRedisCache(IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IConnectionMultiplexer>(x =>
            ConnectionMultiplexer.Connect(config.GetSection("Redis:RedisUrl").Value!));
        
        var redisConnectionString = config.GetConnectionString("Redis");
    }

    private static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "LifeStyles API",
                Version = "v1"
            });
            options.AddServer(new OpenApiServer
            {
                // Url = "/lifestyle-service/"
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

    private static void ConfigureMediatR(IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });
    }

    private static void ConfigureCors(IServiceCollection services)
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

    private static void AddServiceDependencies(IServiceCollection services)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        // services.AddScoped<IRedisCache, RedisCache>();
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("LifeStyleDb");

        services.AddDbContext<LifeStylesDbContext>((sp, opt) =>
        {
            opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            opt.UseNpgsql(connectionString);
        });

        services.AddScoped<DbContext, LifeStylesDbContext>();
    }
}