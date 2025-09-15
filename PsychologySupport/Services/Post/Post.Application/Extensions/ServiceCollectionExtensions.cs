using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Post.Application.Services;
using FluentValidation;
using System.Reflection;

namespace Post.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Add MediatR with behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Add FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Add application services
        services.AddScoped<IRateLimitingService, MemoryRateLimitingService>();
        services.AddScoped<ICounterSynchronizationService, CounterSynchronizationService>();
        
        // Add background services
        services.AddHostedService<PostAbandonmentBackgroundService>();

        // Add options
        services.Configure<RateLimitOptions>(configuration.GetSection(RateLimitOptions.SectionName));
        services.Configure<PostAbandonmentOptions>(configuration.GetSection(PostAbandonmentOptions.SectionName));

        return services;
    }
}
