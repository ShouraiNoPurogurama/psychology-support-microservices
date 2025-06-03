using BuildingBlocks.Exceptions.Handler;
using Carter;
using FluentValidation;
using Promotion.Grpc;
using Scheduling.API.Validators;

namespace Scheduling.API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddCarter();

            services.RegisterMapsterConfiguration();

            services.AddExceptionHandler<CustomExceptionHandler>();

            ConfigureSwagger(services);

            ConfigureCors(services);

            ConfigureMediatR(services);

            AddDatabase(services, config);

            AddServiceDependencies(services);

            AddGrpcServiceDependencies(services, config);

            AddValidatorDependencies(services);

            services.AddMessageBroker(config, typeof(IAssemblyMarker).Assembly);

            return services;
        }

        private static void AddValidatorDependencies(IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<RegisterDoctorBusyAllDayValidator>();
            services.AddValidatorsFromAssemblyContaining<CreateBookingValidator>();
            services.AddValidatorsFromAssemblyContaining<CreateDoctorAvailabilityValidator>();
        }

        private static IHttpClientBuilder AddGrpcServiceDependencies(IServiceCollection services, IConfiguration config)
        {
            return services.AddGrpcClient<PromotionService.PromotionServiceClient>(options =>
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
        }

        private static void AddServiceDependencies(IServiceCollection services)
        {
            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
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

        private static void ConfigureSwagger(IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Scheduling API",
                    Version = "v1"
                });
                options.AddServer(new Microsoft.OpenApi.Models.OpenApiServer
                {
                    Url = "/scheduling-service/"
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
            var connectionString = config.GetConnectionString("SchedulingDb");

            services.AddDbContext<SchedulingDbContext>((sp, opt) =>
            {
                opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                opt.UseNpgsql(connectionString);
            });
        }
    }
}