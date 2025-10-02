using Microsoft.EntityFrameworkCore.Diagnostics;
using Notification.API.Features.Emails.Contracts;
using Notification.API.Features.Emails.Services;
using Notification.API.Infrastructure.Data;

namespace Notification.API.Extensions;

public static class EmailFeatureExtensions
{
    public static IServiceCollection ConfigureEmailFeature(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IEmailService, EmailService>();

        var appSettings = config.Get<AppSettings>();

        services.AddDbContext<NotificationDbContext>((sp, opts) =>
        {
            if (appSettings is null)
            {
                throw new ArgumentException(nameof(AppSettings));
            }

            var dbContextConfig = appSettings.ServiceDbContext;
            opts.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());            
            opts.UseNpgsql(dbContextConfig.NotificationDb);
            opts.UseSnakeCaseNamingConvention();
        });
        return services;
    }
}