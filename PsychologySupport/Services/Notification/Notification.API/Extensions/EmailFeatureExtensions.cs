using Microsoft.EntityFrameworkCore.Diagnostics;
using Notification.API.Data;
using Notification.API.Emails.ServiceContracts;
using Notification.API.Emails.Services;

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
        });
        return services;
    }
}