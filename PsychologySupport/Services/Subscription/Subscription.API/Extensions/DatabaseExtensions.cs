using Microsoft.EntityFrameworkCore;
using Subscription.API.Data;

namespace Subscription.API.Extensions;

public static class DatabaseExtensions
{
    public static void InitializeDatabaseAsync(this WebApplication app)
    {
        // using var scope = app.Services.CreateScope();

        // var context = scope.ServiceProvider.GetRequiredService<SubscriptionDbContext>();

        //context.Database.MigrateAsync().GetAwaiter().GetResult();

        // await SeedAsync(context);
    }
}