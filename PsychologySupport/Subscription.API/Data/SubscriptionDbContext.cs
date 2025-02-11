using Microsoft.EntityFrameworkCore;
using Subscription.API.Models;

namespace Subscription.API.Data
{
    public class SubscriptionDbContext : DbContext
    {
        public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options) : base(options)
        {
        }

        public DbSet<ServicePackage> ServicePackages => Set<ServicePackage>();
        public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("public");

            base.OnModelCreating(builder);
        }
    }
}
