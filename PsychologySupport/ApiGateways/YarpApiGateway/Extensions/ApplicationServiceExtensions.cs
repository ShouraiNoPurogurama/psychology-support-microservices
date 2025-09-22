using Microsoft.AspNetCore.RateLimiting;
using Pii.API.Protos;
using StackExchange.Redis;
using Yarp.ReverseProxy.Transforms.Builder;
using YarpApiGateway.Features.TokenExchange;
using YarpApiGateway.Features.TokenExchange.Contracts;
using YarpApiGateway.Features.TokenExchange.Decorators;
using YarpApiGateway.Features.TokenExchange.Rules;
using YarpApiGateway.Infrastructure;

namespace YarpApiGateway.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
        services.AddHealthChecks();

        ConfigureCors(services);

        ConfigureReverseProxy(services, environment);

        ConfigureRateLimiter(services);

        AddApplicationServiceDependencies(services);

        AddGrpcServiceDependencies(services, configuration);

        AddRedisCache(services, configuration);

        services.AddMemoryCache(options => { options.SizeLimit = 200_000; });

        return services;
    }

    private static void ConfigureReverseProxy(IServiceCollection services, IWebHostEnvironment environment)
    {
        var proxyBuilder = services.AddReverseProxy();

        var envPrefix = environment.EnvironmentName;

        foreach (var file in Directory.GetFiles($"ReverseProxies/{envPrefix}", "*.proxy.json"))
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

    private static void AddApplicationServiceDependencies(IServiceCollection services)
    {
        services.AddSingleton<ITransformFactory, TokenExchangeTransformFactory>();
        services.AddScoped<ITokenExchangeService, TokenExchangeService>();

        services.AddScoped<IInternalTokenMintingService, InternalTokenMintingService>();
        services.Decorate<IInternalTokenMintingService, CachedInternalTokenMintingService>();

        services.AddScoped<TokenExchangeRuleRegistry>();
        services.AddScoped<IPiiLookupService, GrpcPiiLookupService>();
        services.Decorate<IPiiLookupService, CachingPiiLookupService>();
    }

    private static void AddGrpcServiceDependencies(IServiceCollection services, IConfiguration config)
    {
        services.AddGrpcClient<PiiService.PiiServiceClient>(options =>
            {
                options.Address = new Uri(config["GrpcSettings:PiiUrl"]!);
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

    private static void AddRedisCache(IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IConnectionMultiplexer>(x =>
            ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!));

        services.AddStackExchangeRedisCache(options => { options.Configuration = config.GetConnectionString("Redis"); });
    }
}