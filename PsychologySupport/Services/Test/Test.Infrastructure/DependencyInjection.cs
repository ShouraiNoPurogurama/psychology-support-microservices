using BuildingBlocks.Data.Interceptors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QuestPDF.Infrastructure;
using Test.Application.Data;
using Test.Application.ServiceContracts;
using Test.Infrastructure.Data;
using Test.Infrastructure.Data.Extensions;
using Test.Infrastructure.Options;
using Test.Infrastructure.Services;
using Test.Infrastructure.Services.Pdf;

namespace Test.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("TestDb");

        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.EnableDebugging = true;

        
        services.AddDbContext<TestDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString);
            options.UseSnakeCaseNamingConvention();
            options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
        });

        services.AddScoped<ITestDbContext, TestDbContext>();
        services.AddScoped<IAIClient, GeminiClient>();
        services.AddTransient<ITestResultPdfService, TestResultPdfService>();
        
        services.Configure<Dass21PercentileOptions>(
            config.GetSection("Dass21Percentile"));
        services.AddSingleton<Dass21PercentileLookup>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<Dass21PercentileOptions>>().Value;
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            var absolutePath = Path.Combine(env.ContentRootPath, options.CsvPath);
            return new Dass21PercentileLookup(absolutePath);
        });
        
        services.AddHttpClient();

        services.AddIdentityServices(config);

        // Đăng ký Interceptors
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        return services;
    }
}