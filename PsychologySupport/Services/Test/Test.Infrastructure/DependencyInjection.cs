using BuildingBlocks.Data.Interceptors;
using BuildingBlocks.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Profile.API.Protos;
using QuestPDF.Infrastructure;
using Test.Application.Data;
using Test.Application.ServiceContracts;
using Test.Infrastructure.Data;
using Test.Infrastructure.Data.Extensions;
using Test.Infrastructure.Options;
using Test.Infrastructure.Services;
using Test.Infrastructure.Services.Pdf;
using Translation.API.Protos;

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
        
        services.AddRedisCache(config);

        services.AddScoped<ITestDbContext, TestDbContext>();
        services.AddScoped<IAIClient, GeminiClient>();
        services.AddScoped<ICurrentActorAccessor, CurrentActorAccessor>();
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
        
        services.AddGrpc();

        AddGrpcServiceDependencies(services, config);

        return services;
    }
    
    private static void AddGrpcServiceDependencies(IServiceCollection services, IConfiguration config)
    {
        services.AddGrpcClient<PersonaOrchestratorService.PersonaOrchestratorServiceClient>(options =>
            {
                options.Address = new Uri(config["GrpcSettings:PersonaOrchestrationUrl"]!);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            });
        
        services.AddGrpcClient<TranslationService.TranslationServiceClient>(options =>
            {
                options.Address = new Uri(config["GrpcSettings:TranslationUrl"]!);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = true
            });
    }
}