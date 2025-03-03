using LifeStyles.API.Data;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.Extensions;

public static class DatabaseExtensions
{
    public static void InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<LifeStylesDbContext>();

        context.Database.MigrateAsync().GetAwaiter().GetResult();

        // await SeedAsync(context);
    }
}