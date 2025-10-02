using BuildingBlocks.Behaviors;
using BuildingBlocks.Data.Interceptors;
using BuildingBlocks.Exceptions.Handler;
using Carter;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;
using Translation.API.Protos;
using Microsoft.EntityFrameworkCore;
using DigitalGoods.API.Data;
using BuildingBlocks.Messaging.MassTransit;

namespace DigitalGoods.API.Extensions
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

            services.AddMessageBroker(config, typeof(IAssemblyMarker).Assembly);

            return services;
        }

        private static void AddGrpcServiceDependencies(IServiceCollection services, IConfiguration config)
        {
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
                    Title = "DigitalGoods API",
                    Version = "v1"
                });

                //var url = env.IsProduction() 
                //    ? "/digitalgoods-service" 
                //    : "https://localhost:5510/digitalgoods-service";
                
                //options.AddServer(new Microsoft.OpenApi.Models.OpenApiServer
                //{
                //    Url = url
                //});

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
            });
        }

        private static void AddServiceDependencies(IServiceCollection services)
        {
            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        }

        private static void AddDatabase(IServiceCollection services, IConfiguration config)
        {
            var connectionString = GetConnectionString(config);

            services.AddDbContext<DigitalGoodsDbContext>((sp, opt) =>
            {
                opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                opt.UseNpgsql(connectionString);
                opt.UseSnakeCaseNamingConvention();
            });
        }

        private static string? GetConnectionString(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("DigitalGoodsDb");
            return connectionString;
        }
    }
}
