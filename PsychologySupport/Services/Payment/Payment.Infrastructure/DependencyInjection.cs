using BuildingBlocks.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Data;
using Payment.Application.ServiceContracts;
using Payment.Infrastructure.Data;
using Payment.Infrastructure.Extensions;
using Payment.Infrastructure.Services;
using Net.payOS;
using Payment.Application.Utils;

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

        services.AddIdentityServices(config);

        services.AddDbContext<PaymentDbContext>((serviceProvider, options) =>
        {
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString);
            options.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IVnPayService, VNPayService>();
        services.AddScoped<IPaymentValidatorService, PaymentValidatorService>();

        services.AddSingleton(_ =>
        new PayOS(
            config["PayOS:ClientId"]!,
            config["PayOS:ApiKey"]!,
            config["PayOS:ChecksumKey"]!
        )
        );
        services.AddSingleton<PayOSLibrary>();
        services.AddScoped<IPayOSService, PayOSService>();

        return services;
    }
}