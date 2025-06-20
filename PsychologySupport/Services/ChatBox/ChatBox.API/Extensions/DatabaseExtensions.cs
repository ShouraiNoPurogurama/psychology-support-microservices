using ChatBox.API.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatBox.API.Extensions;

public static class DatabaseExtensions
{
    public static void InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        
        var context = scope.ServiceProvider.GetRequiredService<ChatBoxDbContext>();
        
        context.Database.MigrateAsync().GetAwaiter().GetResult();

        // await SeedAsync(context);
    }
}