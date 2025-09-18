using Azure.Storage.Blobs;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Data.Interceptors;
using Carter;
using Image.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;
using Image.API.ServiceContracts;
using Image.API.Services;
using System.Text.Json.Serialization;

namespace Image.API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config,
            IWebHostEnvironment env)
        {
            var connectionString = GetConnectionString(config)!;
            
            services.AddHealthChecks()
                .AddNpgSql(connectionString);
            
            services.AddCarter();
            services.AddControllers()
                .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
            services.AddEndpointsApiExplorer();

            ConfigureSwagger(services, env);
            ConfigureCORS(services);
            ConfigureMediatR(services);
            AddDatabase(services, config);
            AddServiceDependencies(services);
            AddBlobStorage(services, config);

            services.AddHttpContextAccessor();
            //services.AddMessageBroker(config, typeof(IAssemblyMarker).Assembly);

            return services;
        }

        private static void ConfigureSwagger(IServiceCollection services, IWebHostEnvironment env)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Image API",
                    Version = "v1"
                });
                options.AddServer(new OpenApiServer
                {
                    Url = "/image-service/"
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
                options.AddOpenBehavior(typeof(ValidationBehavior<,>));
                options.AddOpenBehavior(typeof(LoggingBehavior<,>));
            });
        }

        private static void AddServiceDependencies(IServiceCollection services)
        {
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        }

        private static void AddDatabase(IServiceCollection services, IConfiguration config)
        {
            var connectionString = GetConnectionString(config);

            services.AddDbContext<ImageDbContext>((sp, opt) =>
            {
                opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                opt.UseNpgsql(connectionString);
                opt.UseSnakeCaseNamingConvention();
            });
        }

        private static string? GetConnectionString(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("ImageDb");
            return connectionString;
        }

        private static void AddBlobStorage(IServiceCollection services, IConfiguration config)
        {
            var blobConnectionString = config["AzureBlobStorage:ConnectionString"];

            if (string.IsNullOrWhiteSpace(blobConnectionString))
            {
                throw new ArgumentNullException(nameof(blobConnectionString), "Azure Blob Storage connection string is missing.");
            }

            services.AddSingleton(new BlobServiceClient(blobConnectionString));
        }
    }
}