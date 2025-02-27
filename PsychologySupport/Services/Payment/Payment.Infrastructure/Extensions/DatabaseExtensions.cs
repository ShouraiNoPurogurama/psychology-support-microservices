using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Payment.Infrastructure.Data;

namespace Payment.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

        context.Database.MigrateAsync().GetAwaiter().GetResult();

        // await SeedAsync(context);
    }

    // private static async Task SeedAsync(PaymentDbContext context)
    // {
    //
    // }
}