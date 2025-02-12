using Microsoft.EntityFrameworkCore;
using Pricing.API.Models;

namespace Pricing.API.Data
{
    public class PricingDbContext : DbContext
    {
        public PricingDbContext(DbContextOptions<PricingDbContext> options) : base(options)
        {
        }
        public DbSet<AcademicLevelSalaryRatio> AcademicLevelSalaryRatios => Set<AcademicLevelSalaryRatio>();
        public DbSet<ExperiencePriceRange> ExperiencePriceRanges => Set<ExperiencePriceRange>();


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("public");

            base.OnModelCreating(builder);
        }
    }
}
