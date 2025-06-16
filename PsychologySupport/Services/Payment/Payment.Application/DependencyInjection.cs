﻿using System.Reflection;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Messaging.Masstransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;

namespace Payment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });
        
        services.AddMessageBroker(configuration, Assembly.GetExecutingAssembly());
        services.AddFeatureManagement();
        services.AddHttpContextAccessor();

        return services;
    }
}