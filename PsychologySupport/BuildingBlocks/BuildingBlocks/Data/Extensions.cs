using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Data;

public static class Extensions
{
    public static IApplicationBuilder UseMigration<TContext>(this IApplicationBuilder app) 
        where TContext : DbContext
    {
        //1. Migrate the db 
        //2. Seed the data
        MigrateDatabaseAsync<TContext>(app.ApplicationServices).GetAwaiter().GetResult();

        // SeedDataAsync(app.ApplicationServices).GetAwaiter().GetResult();

        return app;
    }
    
    private static async Task MigrateDatabaseAsync<TContext>(IServiceProvider serviceProvider)
        where TContext : DbContext
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        await context.Database.MigrateAsync();
    }

    // private static async Task SeedDataAsync(IServiceProvider serviceProvider)
    // {
    //     using var scope = serviceProvider.CreateScope();
    //     var seeders = scope.ServiceProvider.GetServices<IDataSeeder>();
    //     foreach (var seeder in seeders)
    //     {
    //         await seeder.SeedAllAsync();
    //     }
    // }
}