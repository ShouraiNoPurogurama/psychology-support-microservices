using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BuildingBlocks.Extensions;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration config)
    {
        var redisConnectionString = config.GetConnectionString("Redis");
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });
        
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString!));

        return services;
    }
}