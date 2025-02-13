using Feedback.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Feedback.API.Data
{
    public class FeedbackDbContext : DbContext
    {
        public FeedbackDbContext(DbContextOptions<FeedbackDbContext> options) : base(options)
        {
        }

        public DbSet<Models.Feedback> Feedbacks => Set<Models.Feedback>();
        public DbSet<AIRecommendation> AIRecommendations => Set<AIRecommendation>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("public");

            base.OnModelCreating(builder);
        }
    }
}
