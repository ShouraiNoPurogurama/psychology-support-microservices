using Microsoft.EntityFrameworkCore;
using Pricing.API.Data;

namespace Pricing.API.Extensions
{
    public static class DatabaseExtensions
    {
        public static void InitializeDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<PricingDbContext>();

            context.Database.MigrateAsync().GetAwaiter().GetResult();

            // await SeedAsync(context);
        }
    }
}
