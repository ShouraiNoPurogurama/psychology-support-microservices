using Microsoft.EntityFrameworkCore;
using Promotion.Grpc.Models;

namespace Promotion.Grpc.Data;

public class PromotionDbContext : DbContext
{
    public DbSet<Models.Promotion> Promotions => Set<Models.Promotion>();
    
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
    
    public DbSet<GiftCode> GiftCodes => Set<GiftCode>();
    
    public DbSet<Models.PromotionType> PromotionTypes => Set<Models.PromotionType>();
    
    public DbSet<PromotionTypeServicePackage> PromotionTypeServicePackages => Set<PromotionTypeServicePackage>();

    public DbSet<PromotionServicePackage> PromotionServicePackages { get; set; }


    public PromotionDbContext(DbContextOptions<PromotionDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<PromotionTypeServicePackage>(e =>
        {
            e.HasKey(x => new { x.PromotionTypeId, x.ServicePackageId });

            e.HasOne(x => x.PromotionType)
                .WithMany(pt => pt.PromotionTypeServicePackages)
                .HasForeignKey(x => x.PromotionTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PromotionServicePackage>(e =>
        {
            e.HasKey(x => new { x.PromotionId, x.ServicePackageId });

            e.HasOne(x => x.Promotion)
                .WithMany(p => p.PromotionServicePackages)
                .HasForeignKey(x => x.PromotionId)
                .OnDelete(DeleteBehavior.Cascade);

        });

        base.OnModelCreating(builder);
    }


}