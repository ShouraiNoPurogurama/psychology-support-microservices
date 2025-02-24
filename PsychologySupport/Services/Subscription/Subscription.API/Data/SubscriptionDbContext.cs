using Microsoft.EntityFrameworkCore;
using Subscription.API.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Subscription.API.Data.Common;

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

           
            builder.Entity<UserSubscription>()
                .Property(e => e.Status)
                .HasConversion(new EnumToStringConverter<SubscriptionStatus>())
                .HasColumnType("VARCHAR(20)"); 

            base.OnModelCreating(builder);
        }
    }
}
