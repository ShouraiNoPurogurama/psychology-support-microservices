using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Wellness.Application.Data;
using Wellness.Domain.Aggregates.Challenges;
using Wellness.Domain.Aggregates.Challenges.Enums;
using Wellness.Domain.Aggregates.IdempotencyKey;
using Wellness.Domain.Aggregates.JournalMoods;
using Wellness.Domain.Aggregates.ModuleSections;
using Wellness.Domain.Aggregates.OutboxMessage;
using Wellness.Domain.Aggregates.ProcessHistory;
using Wellness.Domain.Enums;

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

    public virtual DbSet<Mood> Moods { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Activity>(entity =>
        {

            entity.Property(e => e.ActivityType)
                        .HasConversion(new EnumToStringConverter<ActivityType>())
                        .HasColumnType("VARCHAR(20)")
                        .HasColumnName("status");
        });

        modelBuilder.Entity<Challenge>(entity =>
        {

            entity.Property(e => e.ChallengeType)
                        .HasConversion(new EnumToStringConverter<ChallengeType>())
                        .HasColumnType("VARCHAR(20)")
                        .HasColumnName("status");
        });

        modelBuilder.Entity<ChallengeProgress>(entity =>
        {

            entity.Property(e => e.ProcessStatus)
                        .HasConversion(new EnumToStringConverter<ProcessStatus>())
                        .HasColumnType("VARCHAR(20)")
                        .HasColumnName("status");
        });

        modelBuilder.Entity<ChallengeStepProgress>(entity =>
        {

            entity.Property(e => e.ProcessStatus)
                        .HasConversion(new EnumToStringConverter<ProcessStatus>())
                        .HasColumnType("VARCHAR(20)")
                        .HasColumnName("status");
        });

        modelBuilder.Entity<ArticleProgress>(entity =>
        {

            entity.Property(e => e.ProcessStatus)
                        .HasConversion(new EnumToStringConverter<ProcessStatus>())
                        .HasColumnType("VARCHAR(20)")
                        .HasColumnName("status");
        });

        modelBuilder.Entity<ModuleProgress>(entity =>
        {

            entity.Property(e => e.ProcessStatus)
                        .HasConversion(new EnumToStringConverter<ProcessStatus>())
                        .HasColumnType("VARCHAR(20)")
                        .HasColumnName("status");
        });

        modelBuilder.Entity<ProcessHistory>(entity =>
        {

            entity.Property(e => e.ProcessStatus)
                        .HasConversion(new EnumToStringConverter<ProcessStatus>())
                        .HasColumnType("VARCHAR(20)")
                        .HasColumnName("status");
        });

        modelBuilder.Entity<SectionArticle>(builder =>
        {
            builder.OwnsOne(x => x.Source, sa =>
            {
                sa.Property(p => p.Name).HasColumnName("source_name");
                sa.Property(p => p.Url).HasColumnName("source_url");
                sa.Property(p => p.Description).HasColumnName("source_description");
            });
        });
    }
}
