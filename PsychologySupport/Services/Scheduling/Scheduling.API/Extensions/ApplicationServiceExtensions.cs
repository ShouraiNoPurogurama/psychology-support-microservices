using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Messaging.MassTransit;
using Carter;
using FluentValidation;
using Promotion.Grpc;
using Scheduling.API.Services;
using Scheduling.API.Validators;

namespace Scheduling.API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            var connectionString = GetConnectionString(config)!;
            services.AddHealthChecks()
                .AddNpgSql(connectionString);
            
            services.AddCarter();

            services.RegisterMapsterConfiguration();

            services.AddExceptionHandler<CustomExceptionHandler>();

            ConfigureSwagger(services, env); 

            ConfigureCors(services);

            ConfigureMediatR(services);

            AddDatabase(services, config);

            AddServiceDependencies(services);

            AddGrpcServiceDependencies(services, config);

            AddValidatorDependencies(services);
            
            services.AddHttpContextAccessor();

            services.AddIdentityServices(config);

            services.AddAuthorization();

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
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true
                });
        }

        private static void AddServiceDependencies(IServiceCollection services)
        {
            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
            services.AddScoped<GeminiClient>();
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
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Scheduling API",
                    Version = "v1"
                });

                var url = env.IsProduction() 
                    ? "/scheduling-service/swagger/v1/swagger.json" 
                    : "https://localhost:5510/scheduling-service";
                
                options.AddServer(new Microsoft.OpenApi.Models.OpenApiServer
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

            services.AddDbContext<SchedulingDbContext>((sp, opt) =>
            {
                opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                opt.UseNpgsql(connectionString);
                opt.UseSnakeCaseNamingConvention();
            });
        }

        private static string? GetConnectionString(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("SchedulingDb");
            return connectionString;
        }
    }
}