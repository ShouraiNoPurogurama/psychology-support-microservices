using Microsoft.AspNetCore.RateLimiting;

namespace YarpApiGateway.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks();
        
        ConfigureCors(services);
        
        ConfigureReverseProxy(services);
        
        ConfigureRateLimiter(services);

        return services;
    }

    private static void ConfigureReverseProxy(IServiceCollection services)
    {
        var proxyBuilder = services.AddReverseProxy();

        foreach (var file in Directory.GetFiles("ReverseProxies", "*.proxy.json"))
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(file, optional: false, reloadOnChange: true)
                .Build();

            proxyBuilder.LoadFromConfig(config.GetSection("ReverseProxy"));
        }
    }

    private static void ConfigureRateLimiter(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("fixed", opt =>
            {
                opt.Window = TimeSpan.FromSeconds(10);
                opt.PermitLimit = 15;
            }); //A maximum of 15 requests per each 10 seconds window are allowed
            options.AddFixedWindowLimiter("post_sessions_fixed_window", opt =>
            {
                opt.Window = TimeSpan.FromSeconds(10);
                opt.PermitLimit = 5;
            }); //A maximum of 5 requests per each 10 seconds window are allowed
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
}