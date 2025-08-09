using Auth.API.BackgroundServices;
using Auth.API.Data;
using Auth.API.ServiceContracts;
using Auth.API.Services;
using Auth.API.Validators;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Filters;
using BuildingBlocks.Messaging.MassTransit;
using Carter;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;

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

        // ConfigureMediatR(services);

        AddDatabase(services, config);

        AddServiceDependencies(services);

        services.AddMessageBroker(config, typeof(IAssemblyMarker).Assembly);

        services.AddHostedService<RevokeSessionCleanupService>();

        services.AddHttpContextAccessor();
        
        ConfigureDataProtection(services, config, env);

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
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
        services.AddScoped<LoggingActionFilter>();

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
        });
    }

    private static string? GetConnectionString(IConfiguration config)
    {
        var connectionString = config.GetConnectionString("AuthDb");
        return connectionString;
    }
}