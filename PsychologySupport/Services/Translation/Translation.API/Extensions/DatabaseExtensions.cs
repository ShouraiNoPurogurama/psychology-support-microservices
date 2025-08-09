using Microsoft.EntityFrameworkCore;
using Translation.API.Data;

namespace Translation.API.Extensions;

public static class DatabaseExtensions
{
    public static void InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        var context = scope.ServiceProvider.GetRequiredService<TranslationDbContext>();
        
        context.Database.MigrateAsync().GetAwaiter().GetResult();

        // await SeedAsync(context);
    }
}