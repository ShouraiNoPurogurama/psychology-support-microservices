using Blog.API.Data;
using BuildingBlocks.Behaviors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;

namespace Blog.API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            // services.AddCarter();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Blog API",
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
            var connectionString = config.GetConnectionString("BlogDb");

            services.AddDbContext<BlogDbContext>((sp, opt) =>
            {
                opt.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                opt.UseNpgsql(connectionString);
            });
        }
    }
}
