using Microsoft.EntityFrameworkCore;

namespace Promotion.Grpc.Data;

public static class Extensions
{
    public static void UseMigration(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<PromotionDbContext>();
        
        dbContext.Database.MigrateAsync().GetAwaiter().GetResult();
    }
}