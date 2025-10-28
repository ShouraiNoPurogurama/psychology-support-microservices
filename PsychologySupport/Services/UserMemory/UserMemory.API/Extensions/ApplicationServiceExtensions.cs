using BuildingBlocks.Behaviors;
using BuildingBlocks.Data.Interceptors;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Messaging.MassTransit;
using Carter;
using Chatbox.API.Protos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;
using Quartz;
using StackExchange.Redis;
using UserMemory.API.Data;
using UserMemory.API.Data.Processors;
using UserMemory.API.Shared.Authentication;
using UserMemory.API.Shared.Outbox;
using UserMemory.API.Shared.Services;
using UserMemory.API.Shared.Services.Contracts;

namespace UserMemory.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config,
        IWebHostEnvironment env)
    {
        var connectionString = GetConnectionString(config)!;
        services.AddHealthChecks()
            .AddNpgSql(connectionString);

        services.AddCarter();

        // services.RegisterMapsterConfiguration();

        services.AddExceptionHandler<CustomExceptionHandler>();

        ConfigureSwagger(services, env);

        ConfigureCors(services);

        ConfigureMediatR(services);

        AddDatabases(services, config);

        AddServiceDependencies(services);
        
        AddRedisCache(services, config);

        AddGrpcServiceDependencies(services, config);
        
        ConfigureGrpc(services);
        
        ConfigureHttpClient(services, config);
        
        ConfigureCronJobs(services);
        
        services.AddHostedService<OutboxProcessor>();
        
        services.AddHttpContextAccessor();

        services.AddHttpClient();

        services.AddIdentityServices(config);

        services.AddAuthorization();

        services.AddMessageBroker(config, typeof(IAssemblyMarker).Assembly, null, "user-memory");

        return services;
    }

    private static void AddRedisCache(IServiceCollection services, IConfiguration config)
    {
        var redisConnectionString = config.GetConnectionString("Redis");
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });
        
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString!));
    }


    private static void AddServiceDependencies(IServiceCollection services)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddScoped<ICurrentActorAccessor, CurrentActorAccessor>();
        services.AddMemoryCache();
        services.AddScoped<IOutboxWriter, EfOutboxWriter>();
        services.AddScoped<IEmbeddingService, GeminiEmbeddingService>();
        services.AddScoped<IGeminiClient, GeminiClient>();
        services.AddScoped<ITagResolverService, TagResolverService>();
        services.AddScoped<ITagSyncService, TagSyncService>();
        
        services.AddSingleton<IGeminiService, GeminiService>();
        services.AddScoped<IRewardPromptGenerator, ChatAndAiPromptGenerator>();
        services.AddScoped<ICurrentUserSubscriptionAccessor, CurrentCurrentUserSubscriptionAccessor>();
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

    private static void ConfigureSwagger(IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "User Memory API",
                Version = "v1"
            });

            var url = env.IsProduction() 
                ? "/user-memory-service" 
                : "https://localhost:5510/user-memory-service";
            
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

    private static void AddDatabases(IServiceCollection services, IConfiguration config)
    {
        var connectionString = GetConnectionString(config);

        services.AddDbContext<UserMemoryDbContext>((sp, opt) =>
        {
            opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            opt.UseNpgsql(connectionString, o => o.UseVector());
            opt.UseSnakeCaseNamingConvention();
        });
    }
    
    private static void AddGrpcServiceDependencies(IServiceCollection services, IConfiguration config)
    {
        services.AddGrpcClient<ChatboxService.ChatboxServiceClient>(options =>
            {
                options.Address = new Uri(config["GrpcSettings:ChatboxUrl"]!);
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

    
    private static void ConfigureGrpc(IServiceCollection services)
    {
        services.AddGrpc();
    }
    
    private static void ConfigureHttpClient(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IMediaServiceClient, HttpMediaServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:MediaServiceUrl"]);
            client.Timeout = TimeSpan.FromSeconds(120); // Job tạo ảnh có thể mất nhiều thời gian
        });
    }
    
    private static void ConfigureCronJobs(IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            var updateGlobalFallbackJobKey = new JobKey(ProcessRewardRequestJobConfiguration.JobName);
            
            q.AddJob<ProcessRewardRequestJob>(opts => opts.WithIdentity(updateGlobalFallbackJobKey));

            q.AddTrigger(opts => opts
                .ForJob(updateGlobalFallbackJobKey)
                .WithIdentity($"{ProcessRewardRequestJobConfiguration.JobName}-trigger")
                .StartNow()
                .WithCronSchedule(ProcessRewardRequestJobConfiguration.CronExpression));
        });
        
        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
    }

    private static string? GetConnectionString(IConfiguration config)
    {
        var connectionString = config.GetConnectionString("UserMemoryDb");
        return connectionString;
    }
}

