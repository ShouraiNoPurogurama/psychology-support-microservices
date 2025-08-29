using AIModeration.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AIModeration.API.Data;

public partial class ModerationdbContext : DbContext
{
    public ModerationdbContext()
    {
    }

    public ModerationdbContext(DbContextOptions<ModerationdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ModerationAudit> ModerationAudits { get; set; }
    public virtual DbSet<ModerationItem> ModerationItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<ModerationAudit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("moderation_audits_pkey");
            entity.ToTable("moderation_audits");

            entity.HasIndex(e => new { e.ItemId, e.CreatedAt }, "idx_moderation_audits_item");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Data).HasColumnType("jsonb");

            entity.HasOne(d => d.Item)
                .WithMany(p => p.ModerationAudits)
                .HasForeignKey(d => d.ItemId)
                .HasConstraintName("moderation_audits_item_id_fkey");
        });

        modelBuilder.Entity<ModerationItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("moderation_items_pkey");
            entity.ToTable("moderation_items");

            entity.HasIndex(e => e.ContentHash, "idx_moderation_items_hash");
            entity.HasIndex(e => new { e.TargetType, e.TargetId }, "idx_moderation_items_lookup");
            entity.HasIndex(e => new { e.Status, e.LastModified }, "idx_moderation_items_status")
                .IsDescending(false, true);
            entity.HasIndex(e => new { e.TargetType, e.TargetId, e.ContentHash }, "uq_moderation_items_snapshot")
                .IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Scores).HasColumnType("jsonb");
            entity.Property(e => e.PolicyVersion).HasDefaultValueSql("'v1'::text");
            entity.Property(e => e.Status).HasDefaultValueSql("'pending'::text");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}