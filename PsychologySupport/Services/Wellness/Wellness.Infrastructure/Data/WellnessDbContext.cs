using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Domain.Models;


namespace Wellness.Infrastructure.Data;

public partial class WellnessDbContext : DbContext, IWellnessDbContext
{
    public WellnessDbContext(DbContextOptions<WellnessDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activity> Activities { get; set; }

    public virtual DbSet<ArticleProgress> ArticleProgresses { get; set; }

    public virtual DbSet<Challenge> Challenges { get; set; }

    public virtual DbSet<ChallengeProgress> ChallengeProgresses { get; set; }

    public virtual DbSet<ChallengeStep> ChallengeSteps { get; set; }

    public virtual DbSet<ChallengeStepProgress> ChallengeStepProgresses { get; set; }

    public virtual DbSet<IdempotencyKey> IdempotencyKeys { get; set; }

    public virtual DbSet<JournalMood> JournalMoods { get; set; }

    public virtual DbSet<ModuleProgress> ModuleProgresses { get; set; }

    public virtual DbSet<ModuleSection> ModuleSections { get; set; }

    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }

    public virtual DbSet<ProcessHistory> ProcessHistories { get; set; }

    public virtual DbSet<SectionArticle> SectionArticles { get; set; }

    public virtual DbSet<WellnessModule> WellnessModules { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
