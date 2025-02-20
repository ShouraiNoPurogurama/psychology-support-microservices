using Microsoft.EntityFrameworkCore;
using Notification.API.Data;

namespace Notification.API.Extensions;

public static class DatabaseExtensions
{
    public static void InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        context.Database.MigrateAsync().GetAwaiter().GetResult();

        // await SeedAsync(context);
    }
}