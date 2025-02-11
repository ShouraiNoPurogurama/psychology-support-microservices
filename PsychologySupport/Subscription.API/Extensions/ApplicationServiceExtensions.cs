using BuildingBlocks.Behaviors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;
using Subscription.API.Data;

namespace Subscription.API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            // services.AddCarter();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Subscription API",
                Version = "v1"
            }));


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

            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
                configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
                configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            });

            AddDatabase(services, config);

            AddServiceDependencies(services);

            return services;
        }

        private static void AddServiceDependencies(IServiceCollection services)
        {

        }

        private static void AddDatabase(IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("SubscriptionDb");

            services.AddDbContext<SubscriptionDbContext>((sp, opt) =>
            {
                opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                opt.UseNpgsql(connectionString);
            });
        }
    }
}
