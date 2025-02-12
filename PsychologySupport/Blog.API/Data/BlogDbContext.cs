using Microsoft.EntityFrameworkCore;

namespace Blog.API.Data
{
    public class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
        {
        }

        public DbSet<Models.Blog> Blogs => Set<Models.Blog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("public");

            base.OnModelCreating(builder);
        }
    }
}
