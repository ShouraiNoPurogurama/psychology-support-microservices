using Microsoft.EntityFrameworkCore;
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

        services.AddDbContext<NotificationDbContext>(options =>
        {
            if (appSettings is null)
            {
                throw new ArgumentException(nameof(AppSettings));
            }

            var dbContextConfig = appSettings.ServiceDbContext;
            
            options.UseNpgsql(dbContextConfig.NotificationDb);
        });
        return services;
    }
}