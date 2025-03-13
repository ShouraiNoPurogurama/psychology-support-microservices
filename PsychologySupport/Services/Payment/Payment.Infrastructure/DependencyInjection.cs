using BuildingBlocks.Data.Interceptors;
using BuildingBlocks.Messaging.Masstransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Data;
using Payment.Application.ServiceContracts;
using Payment.Infrastructure.Data;
using Payment.Infrastructure.Extensions;
using Payment.Infrastructure.Services;

namespace Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        
        var connectionString = config.GetConnectionString("PaymentDb");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        services.AddScoped<IPaymentDbContext, PaymentDbContext>();
        services.RegisterMapsterConfiguration();
        
        services.AddDbContext<PaymentDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IVnPayService, VNPayService>();
        services.AddScoped<IPaymentValidatorService, PaymentValidatorService>();
        
        return services;
    }
}