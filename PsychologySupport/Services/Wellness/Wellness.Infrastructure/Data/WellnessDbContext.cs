using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Wellness.Application.Data;
using Wellness.Domain.Aggregates.Challenges;
using Wellness.Domain.Aggregates.Challenges.Enums;
using Wellness.Domain.Aggregates.IdempotencyKey;
using Wellness.Domain.Aggregates.JournalMoods;
using Wellness.Domain.Aggregates.ModuleSections;
using Wellness.Domain.Aggregates.ModuleSections.Enums;
using Wellness.Domain.Aggregates.OutboxMessage;
using Wellness.Domain.Aggregates.ProcessHistories;
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
                        .HasColumnName("activity_type");
        });

        modelBuilder.Entity<Challenge>(entity =>
        {

            entity.Property(e => e.ChallengeType)
                        .HasConversion(new EnumToStringConverter<ChallengeType>())
                        .HasColumnType("VARCHAR(20)")
                        .HasColumnName("challenge_type");

            entity.Property(e => e.ImprovementTag)
              .HasConversion(new EnumToStringConverter<ImprovementTag>())
              .HasColumnType("VARCHAR(30)")
              .HasColumnName("improvement_tag");

            entity.Property(e => e.Scope)
              .HasConversion(new EnumToStringConverter<TagScope>())
              .HasColumnType("VARCHAR(20)")
              .HasColumnName("scope");

            entity.Property(e => e.MediaId)
               .HasColumnName("media_id")
               .HasColumnType("uuid")
               .IsRequired(false);
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

            builder.Property(e => e.Scope)
              .HasConversion(new EnumToStringConverter<TagScope>())
              .HasColumnType("VARCHAR(20)")
              .HasColumnName("scope");
        });

        modelBuilder.Entity<ModuleSection>(entity =>
        {
            entity.Property(e => e.Category)
                .HasConversion(new EnumToStringConverter<ModuleCategory>())
                .HasColumnType("VARCHAR(30)")
                .HasColumnName("category");
        });
    }
}
