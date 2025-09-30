using System.Net;
using System.Net.Http;
using Auth.API.Common.Authentication;
using Auth.API.Data;
using Auth.API.Features.Authentication.BackgroundServices;
using Auth.API.Features.Authentication.ServiceContracts;
using Auth.API.Features.Authentication.ServiceContracts.Features;
using Auth.API.Features.Authentication.ServiceContracts.Shared;
using Auth.API.Features.Authentication.Services;
using Auth.API.Features.Authentication.Services.Features;
using Auth.API.Features.Authentication.Services.Shared;
using Auth.API.Features.Authentication.Validators;
using Auth.API.Features.Encryption.ServiceContracts;
using Auth.API.Features.Encryption.Services;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Data.Interceptors;
using BuildingBlocks.Extensions;
using BuildingBlocks.Filters;
using BuildingBlocks.Messaging.MassTransit;
using Carter;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;
using Notification.API.Protos;
using Pii.API.Protos;
using Profile.API.Protos;
using EmailService = Auth.API.Features.Authentication.Services.Shared.EmailService;

namespace Auth.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config,
        IWebHostEnvironment env)
    {
        services.AddCarter();
        services.AddControllers(options => { options.Filters.Add<LoggingActionFilter>(); });
        services.AddEndpointsApiExplorer();

        var connectionString = GetConnectionString(config)!;
        services.AddHealthChecks()
            .AddNpgSql(connectionString);

        ConfigureSwagger(services, env);

        ConfigureCors(services);

        ConfigureMediatR(services);

        AddDatabase(services, config);

        AddServiceDependencies(services);

        services.AddRedisCache(config);

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
            
            var url = env.IsProduction() 
                ? "/auth-service/swagger/v1/swagger.json" 
                : "https://localhost:5510/auth-service";

            //options.AddServer(new OpenApiServer
            //{
            //    Url = url
            //});
        });
    }

    private static void AddServiceDependencies(IServiceCollection services)
    {
        services.AddScoped<LoggingActionFilter>();
        services.AddScoped<IPayloadProtector, PayloadProtector>();
        services.AddScoped<ICurrentActorAccessor, CurrentActorAccessor>();

        //Facade
        services.AddScoped<IAuthFacade, AuthFacade>();
        services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();

        //Features
        services.AddScoped<IUserRegistrationService, UserRegistrationService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IUserAccountService, UserAccountService>();
        services.AddScoped<IUserOnboardingService, UserOnboardingService>();

        //Shared
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IDeviceManagementService, DeviceManagementService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPayloadProtector, PayloadProtector>();
        services.AddScoped<IUserProvisioningService, UserProvisioningService>();
        services.AddScoped<ITokenRevocationService, TokenRevocationService>();
        services.Decorate<ITokenRevocationService, CachedTokenRevocationService>();

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
        var notifUrl = config["GrpcSettings:NotificationUrl"];
        var profileUrl = config["GrpcSettings:PatientProfileUrl"];
        var piiUrl = config["GrpcSettings:PiiUrl"];

        services.AddGrpcClient<NotificationService.NotificationServiceClient>(options =>
            {
                options.Address = new Uri(notifUrl!);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            });

        services.AddGrpcClient<PatientProfileService.PatientProfileServiceClient>(options =>
            {
                options.Address = new Uri(profileUrl!);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            });

        services.AddGrpcClient<PiiService.PiiServiceClient>(options => { options.Address = new Uri(piiUrl!); })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            });
    }
}