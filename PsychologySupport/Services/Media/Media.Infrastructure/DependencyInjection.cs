using Azure.Storage.Blobs;
using Media.Application.Data;
using Media.Application.ServiceContracts;
using Media.Infrastructure.Data;
using Media.Infrastructure.Data.Interceptors;
using Media.Infrastructure.Options;
using Media.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Media.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("MediaDb");

            // DbContext + Interceptors
            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
            services.AddScoped<IMediaDbContext, MediaDbContext>();

            services.AddDbContext<MediaDbContext>((serviceProvider, options) =>
            {
                options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());
                options.UseSnakeCaseNamingConvention();
                options.UseNpgsql(connectionString);
            });

            // Azure Blob Storage Configuration
            services.Configure<AzureStorageOptions>(config.GetSection("Azure:BlobStorage"));

            // BlobServiceClient singleton
            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<AzureStorageOptions>>().Value;
                return new BlobServiceClient(options.ConnectionString);
            });

            // Sightengine Configuration
            services.Configure<SightengineOptions>(config.GetSection("Sightengine"));

            // Register ISightengineService with HttpClient
            services.AddHttpClient<ISightengineService, SightengineService>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<SightengineOptions>>().Value;
                if (string.IsNullOrEmpty(options.BaseUrl))
                {
                    throw new ArgumentException("Sightengine BaseUrl is not configured.");
                }
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");

            });

            // Other services
            services.AddScoped<IStorageService, AzureBlobStorageService>();

            return services;
        }
    }
}