using Auth.API.Data;
using Auth.API.Domains.Authentication.BackgroundServices;
using Auth.API.Domains.Authentication.ServiceContracts;
using Auth.API.Domains.Authentication.ServiceContracts.v2;
using Auth.API.Domains.Authentication.Services;
using Auth.API.Domains.Authentication.Services.v2;
using Auth.API.Domains.Authentication.Validators;
using Auth.API.Domains.Encryption.ServiceContracts;
using Auth.API.Domains.Encryption.Services;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Data.Interceptors;
using BuildingBlocks.Filters;
using BuildingBlocks.Messaging.MassTransit;
using Carter;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;
using Notification.API.Protos;
using Profile.API.Protos;
using AuthService = Auth.API.Domains.Authentication.Services.v2.AuthService;
using IAuthService = Auth.API.Domains.Authentication.ServiceContracts.v2.IAuthService;

namespace Auth.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {
        services.AddCarter();
        services.AddControllers(options =>
        {
            options.Filters.Add<LoggingActionFilter>();
        });
        services.AddEndpointsApiExplorer();
        
        var connectionString = GetConnectionString(config)!;
        services.AddHealthChecks()
            .AddNpgSql(connectionString);

        ConfigureSwagger(services, env);

        ConfigureCors(services);

        ConfigureMediatR(services);

        AddDatabase(services, config);

        AddServiceDependencies(services);

        services.AddMessageBroker(config, typeof(IAssemblyMarker).Assembly);

        services.AddHostedService<RevokeSessionCleanupService>();

        services.AddHttpContextAccessor();
        
        ConfigureDataProtection(services, config, env);

        AddGrpcServiceDependencies(services, config);


        return services;
    }

    private static void ConfigureDataProtection(IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {
        var keyRingPath = config["DataProtection:KeyRingPath"]!;

        if (env.IsDevelopment())
        {
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath));
        }
        else
        {
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath));
        }
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
                Title = "Auth API",
                Version = "v1"
            });
            
            //Chỉ add server khi chạy Production
            if (env.IsProduction())
            {
                options.AddServer(new OpenApiServer
                {
                    Url = "/auth-service/"
                });
            }
        });
    }

    private static void AddServiceDependencies(IServiceCollection services)
    {
        services.AddScoped<Domains.Authentication.ServiceContracts.IAuthService, Domains.Authentication.Services.AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
        services.AddScoped<LoggingActionFilter>();
        services.AddScoped<IAuthService, AuthService>();
        services
            .AddScoped<Domains.Authentication.ServiceContracts.v3.IAuthService, Domains.Authentication.Services.v3.AuthService>();
        services.AddScoped<IPayloadProtector, PayloadProtector>();
        
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<ChangePasswordRequestValidator>();
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration config)
    {
        var connectionString = GetConnectionString(config);

        services.AddDbContext<AuthDbContext>((sp, opt) =>
        {
            opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            opt.UseNpgsql(connectionString);
            opt.UseSnakeCaseNamingConvention();
        });
    }

    private static string? GetConnectionString(IConfiguration config)
    {
        var connectionString = config.GetConnectionString("AuthDb");
        return connectionString;
    }

    private static void AddGrpcServiceDependencies(IServiceCollection services, IConfiguration config)
    {
        services.AddGrpcClient<NotificationService.NotificationServiceClient>(options =>
        {
            options.Address = new Uri(config["GrpcSettings:NotificationUrl"]!);
        })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                return handler;
            });
        
        services.AddGrpcClient<PatientProfileService.PatientProfileServiceClient>(options =>
        {
            options.Address = new Uri(config["GrpcSettings:PatientProfileUrl"]!);
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
}