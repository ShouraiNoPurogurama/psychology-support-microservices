using Microsoft.EntityFrameworkCore;
using UserMemory.API.Models;
using UserMemory.API.Shared.Enums;

namespace UserMemory.API.Data;

using UserMemory = Models.UserMemory;

public class UserMemoryDbContext : DbContext
{
    public UserMemoryDbContext(DbContextOptions<UserMemoryDbContext> options) : base(options)
    {
    }

    public virtual DbSet<UserMemory> UserMemories => Set<UserMemory>();
    public virtual DbSet<Reward> Rewards => Set<Reward>();
    public virtual DbSet<SessionDailyProgress> SessionDailyProgresses => Set<SessionDailyProgress>();

    public virtual DbSet<AliasDailySummary> AliasDailySummaries => Set<AliasDailySummary>();
    public virtual DbSet<MemoryTag> MemoryTags => Set<MemoryTag>();
    public virtual DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasPostgresExtension("vector");

        builder.Entity<SessionDailyProgress>(entity => { entity.HasKey(e => new { e.AliasId, e.ProgressDate, e.SessionId }); });

        builder.Entity<Reward>(entity =>
        {
            entity.Property(e => e.Status)
                .HasConversion(r => r.ToString(),
                    dbStatus => Enum.Parse<RewardStatus>(dbStatus)
                )
                .HasSentinel(RewardStatus.Pending)
                .HasDefaultValue(RewardStatus.Pending);
        });

        builder.Entity<UserMemory>(entity =>
        {
            entity.HasMany(um => um.MemoryTags)
                .WithMany(mt => mt.UserMemories)
                .UsingEntity<Dictionary<string, object>>(
                    // NHÁNH Tag (right)
                    right => right
                        .HasOne<MemoryTag>()
                        .WithMany()
                        .HasForeignKey("memory_tag_id")
                        .OnDelete(DeleteBehavior.Cascade),
                    // NHÁNH UserMemory (left)
                    left => left
                        .HasOne<UserMemory>()
                        .WithMany()
                        .HasForeignKey("user_memory_id")
                        .OnDelete(DeleteBehavior.Cascade),
                    // CẤU HÌNH BẢNG JOIN + INDEX
                    join =>
                    {
                        join.ToTable("user_memory_tags");
                        join.HasKey("user_memory_id", "memory_tag_id");

                        // Index chiều ngược để tối ưu cả 2 hướng truy vấn
                        join.HasIndex("user_memory_id")
                            .HasDatabaseName("ix_user_memory_tags_user_memory_id");
                        join.HasIndex("memory_tag_id")
                            .HasDatabaseName("ix_user_memory_tags_memory_tag_id");
                    });

            entity.HasIndex(e => e.AliasId);

            entity.Property(e => e.Embedding)
                .HasColumnType("vector(3072)");
        });

        builder.Entity<MemoryTag>(entity =>
        {
            entity.HasIndex(e => e.Code)
                .IsUnique();
        });

        builder.Entity<AliasDailySummary>(entity => { entity.HasKey(e => new { e.AliasId, e.Date }); });
    }
}