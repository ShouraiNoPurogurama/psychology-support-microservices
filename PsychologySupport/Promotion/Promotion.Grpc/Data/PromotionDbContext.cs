using Microsoft.EntityFrameworkCore;
using Promotion.Grpc.Models;

namespace Promotion.Grpc.Data;

public class PromotionDbContext : DbContext
{
    public DbSet<Models.Promotion> Promotions => Set<Models.Promotion>();
    
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
    
    public DbSet<GiftCode> GiftCodes => Set<GiftCode>();
    
    public DbSet<PromotionType> PromotionTypes => Set<PromotionType>();
    
    public PromotionDbContext(DbContextOptions<PromotionDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}