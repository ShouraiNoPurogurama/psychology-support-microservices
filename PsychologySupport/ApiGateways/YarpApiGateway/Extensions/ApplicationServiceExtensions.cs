using Microsoft.AspNetCore.RateLimiting;

namespace YarpApiGateway.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureCors(services);
        
        // services.AddReverseProxy()
        //     .LoadFromConfig(configuration.GetSection("ReverseProxy"));

        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("fixed", opt =>
            {
                opt.Window = TimeSpan.FromSeconds(10);
                opt.PermitLimit = 10;
            }); //A maximum of 10 requests per each 10 seconds window are allowed
        });

        return services;
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
}