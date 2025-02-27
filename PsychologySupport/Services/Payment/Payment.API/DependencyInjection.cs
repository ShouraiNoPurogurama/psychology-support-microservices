using BuildingBlocks.Exceptions.Handler;
using Carter;

namespace Payment.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCarter();
        services.AddExceptionHandler<CustomExceptionHandler>();
        
        return services;
    }
}