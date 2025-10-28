using BuildingBlocks.Extensions;
using BuildingBlocks.Messaging.MassTransit;
using FluentValidation;
using Notification.API.Protos;
using Profile.API.Data.Pii;
using Profile.API.Domains.Pii.Services;
using Profile.API.Domains.Public.DoctorProfiles.Validators;
using Profile.API.Domains.Public.PatientProfiles.Services;
using Profile.API.Domains.Public.PatientProfiles.Validators;

namespace Profile.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {
        var connectionString = GetConnectionString(config)!;
        services.AddHealthChecks()
            .AddNpgSql(connectionString);
        
        services.AddEndpointsApiExplorer();

        services.AddCarter();

        ConfigureSwagger(services, env);

        ConfigureCors(services);

        ConfigureMediatR(services);

        AddDatabase(services, config);

        AddServiceDependencies(services);
        
        services.AddRedisCache(config);

        services.AddIdentityServices(config);

        services.AddAuthorization();

        services.AddHttpContextAccessor();
        
        services.AddMessageBroker(config, typeof(IAssemblyMarker).Assembly, null, "profile");

        services.AddValidatorsFromAssemblyContaining<UpdateDoctorProfileValidator>();
        
        services.AddValidatorsFromAssemblyContaining<PatchPatientProfileValidator>();

        ConfigureGrpc(services);

        AddGrpcServiceDependencies(services, config);

        return services;
    }

    private static void AddServiceDependencies(IServiceCollection services)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddScoped<PatientProfileLocalService>();
        services.AddScoped<PiiLocalService>();

        services.AddScoped<PiiService>();
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
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Profile API",
                Version = "v1"
            });

            var url = env.IsProduction() 
                ? "/profile-service" 
                : "https://localhost:5510/profile-service";
            
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

    private static void AddDatabase(IServiceCollection services, IConfiguration config)
    {
        var connectionString = GetConnectionString(config);

        services.AddDbContext<ProfileDbContext>((sp, opt) =>
        {
            opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            opt.UseNpgsql(connectionString);
            opt.UseSnakeCaseNamingConvention();
        });
        
        var piiConnectionString = GetConnectionString(config, "PiiProfileDb");
        
        services.AddDbContext<PiiDbContext>((sp, opt) =>
        {
            opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            opt.UseNpgsql(piiConnectionString);
            opt.UseSnakeCaseNamingConvention();
        });
    }

    private static string? GetConnectionString(IConfiguration config, string name = "PublicProfileDb")
    {
        var connectionString = config.GetConnectionString(name);
        return connectionString;
    }

    private static void ConfigureGrpc(IServiceCollection services)
    {
        services.AddGrpc();
        services.AddGrpcReflection();
    }

    private static void AddGrpcServiceDependencies(IServiceCollection services, IConfiguration config)
    {
        services.AddGrpcClient<NotificationService.NotificationServiceClient>(options =>
        {
            options.Address = new Uri(config["GrpcSettings:NotificationUrl"]!);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            EnableMultipleHttp2Connections = true
        });
    }
}