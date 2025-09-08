using BuildingBlocks.Behaviors;
using BuildingBlocks.Data.Interceptors;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Messaging.MassTransit;
using Carter;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Promotion.Grpc;
using Translation.API.Protos;
using Billing.API.Data;
using BuildingBlocks.Services;
using Billing.API.Domains.Idempotency;

namespace Billing.API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            var connectionString = GetConnectionString(config)!;
            services.AddHealthChecks()
                .AddNpgSql(connectionString);

            services.AddCarter();

            services.AddExceptionHandler<CustomExceptionHandler>();

            services.RegisterMapsterConfiguration();

            services.AddIdentityServices(config);

            services.AddAuthorization();

            services.AddHttpContextAccessor();

            ConfigureSwagger(services, env);
            ConfigureCORS(services);
            ConfigureMediatR(services);
            AddDatabase(services, config);
            AddServiceDependencies(services);

            AddGrpcServiceDependencies(services, config);

            //services.AddMessageBroker(config, typeof(IAssemblyMarker).Assembly);

            return services;
        }

        private static void AddGrpcServiceDependencies(IServiceCollection services, IConfiguration config)
        {
            services.AddGrpcClient<PromotionService.PromotionServiceClient>(options =>
            {
                options.Address = new Uri(config["GrpcSettings:PromotionUrl"]!);
            })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                    return handler;
                });
            services.AddGrpcClient<TranslationService.TranslationServiceClient>(options =>
            {
                options.Address = new Uri(config["GrpcSettings:TranslationUrl"]!);
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

        private static void ConfigureSwagger(IServiceCollection services, IWebHostEnvironment env)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Billing API",
                    Version = "v1"
                });

                if (env.IsProduction())
                {
                    options.AddServer(new Microsoft.OpenApi.Models.OpenApiServer
                    {
                        Url = "/billing-service/"
                    });
                }

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
            services.AddMediatR(options =>
            {
                options.RegisterServicesFromAssembly(typeof(Program).Assembly);
                // options.AddOpenBehavior(typeof(ValidationBehavior<,>));
                options.AddOpenBehavior(typeof(LoggingBehavior<,>));
                config.AddOpenBehavior(typeof(IdempotentCommandPipelineBehaviour<,>));
            });
        }

        private static void AddServiceDependencies(IServiceCollection services)
        {
            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
            services.AddScoped<IIdempotencyService, IdempotencyService>();
        }

        private static void AddDatabase(IServiceCollection services, IConfiguration config)
        {
            var connectionString = GetConnectionString(config);

            services.AddDbContext<BillingDbContext>((sp, opt) =>
            {
                opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                opt.UseNpgsql(connectionString);
                opt.UseSnakeCaseNamingConvention();
            });
        }

        private static string? GetConnectionString(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("BillingDb");
            return connectionString;
        }
    }
}
