using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;
using Image.API.Data.Common;

namespace Image.API.Data
{
    public class ImageDbContext : DbContext
    {
        public ImageDbContext(DbContextOptions<ImageDbContext> options) : base(options)
        {
        }

        public DbSet<Models.Image> Images => Set<Models.Image>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("public");

            builder.Entity<Models.Image>()
              .Property(e => e.OwnerType)
              .HasConversion(new EnumToStringConverter<OwnerType>())
              .HasColumnType("VARCHAR(20)");
        }
    }
}
