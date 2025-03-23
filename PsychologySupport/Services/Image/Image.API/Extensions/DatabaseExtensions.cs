using Image.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Image.API.Extensions
{
    public static class DatabaseExtensions
    {
        public static void InitializeDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ImageDbContext>();

            context.Database.MigrateAsync().GetAwaiter().GetResult();

            //await SeedAsync(context);
        }
    }
}
