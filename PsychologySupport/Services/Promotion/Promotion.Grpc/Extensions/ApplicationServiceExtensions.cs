using Microsoft.EntityFrameworkCore;
using Promotion.Grpc.BackgroundServices;
using Promotion.Grpc.Data;
using Promotion.Grpc.Services;

namespace Promotion.Grpc.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {
        ConfigureGrpc(services);

        ConfigureDatabase(services, config);

        ConfigureSwagger(services, env); 
        
        AddServiceDependencies(services);
        
        services.AddHttpContextAccessor();

        return services;
    }

    private static void ConfigureSwagger(IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Promotion API",
                Version = "v1"
            });
            options.AddServer(new Microsoft.OpenApi.Models.OpenApiServer
            {
                Url = "/promotion-service/"
            });
        });
    }

    private static void AddServiceDependencies(IServiceCollection services)
    {
        //Background service configurations
        services.AddHostedService<UpdatePromotionStatusesBackgroundService>();

        services.RegisterMapsterConfigurations();

        services.AddScoped<ValidatorService>();
    }

    private static void ConfigureDatabase(IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("PromotionDb");

        services.AddDbContext<PromotionDbContext>((sp, opt) =>
        {
            opt.UseNpgsql(connectionString);
        });
    }

    private static void ConfigureGrpc(IServiceCollection services)
    {
        // Add services to the container.
        services.AddGrpc();
        services.AddGrpcReflection();
    }
}