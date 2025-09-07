using System;
using System.Collections.Generic;
using Media.API.Media.Models;
using Microsoft.EntityFrameworkCore;

namespace Media.API.Data;

public partial class MediaDbContext : DbContext
{
    public MediaDbContext()
    {
    }

    public MediaDbContext(DbContextOptions<MediaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<IdempotencyKey> IdempotencyKeys { get; set; }

    public virtual DbSet<MediaAsset> MediaAssets { get; set; }

    public virtual DbSet<MediaModerationAudit> MediaModerationAudits { get; set; }

    public virtual DbSet<MediaOwner> MediaOwners { get; set; }

    public virtual DbSet<MediaProcessingJob> MediaProcessingJobs { get; set; }

    public virtual DbSet<MediaVariant> MediaVariants { get; set; }

    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<IdempotencyKey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idempotency_keys_pkey");

            entity.ToTable("idempotency_keys");

            entity.HasIndex(e => e.CreatedAt, "idx_idem_created_at");

            entity.HasIndex(e => e.IdempotencyKey1, "uq_idempotency_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IdempotencyKey1).HasColumnName("idempotency_key");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("now()")
                .HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.RequestHash).HasColumnName("request_hash");
            entity.Property(e => e.ResponseBody)
                .HasColumnType("jsonb")
                .HasColumnName("response_body");
        });

        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("media_assets_pkey");

            entity.ToTable("media_assets");

            entity.HasIndex(e => new { e.ModerationStatus, e.ModerationCheckedAt }, "idx_media_assets_moderation").IsDescending(false, true);

            entity.HasIndex(e => e.State, "idx_media_assets_state");

            entity.HasIndex(e => e.ChecksumSha256, "uq_media_assets_checksum").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ChecksumSha256)
                .HasMaxLength(64)
                .IsFixedLength()
                .HasColumnName("checksum_sha256");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.ExifRemoved)
                .HasDefaultValue(true)
                .HasColumnName("exif_removed");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.HoldThumbUntilPass)
                .HasDefaultValue(false)
                .HasColumnName("hold_thumb_until_pass");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("now()")
                .HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.ModerationCheckedAt).HasColumnName("moderation_checked_at");
            entity.Property(e => e.ModerationPolicyVersion).HasColumnName("moderation_policy_version");
            entity.Property(e => e.ModerationScore)
                .HasPrecision(5, 4)
                .HasColumnName("moderation_score");
            entity.Property(e => e.ModerationStatus)
                .HasDefaultValueSql("'pending'::text")
                .HasColumnName("moderation_status");
            entity.Property(e => e.Phash64)
                .HasMaxLength(16)
                .IsFixedLength()
                .HasColumnName("phash64");
            entity.Property(e => e.RawModerationJson)
                .HasColumnType("jsonb")
                .HasColumnName("raw_moderation_json");
            entity.Property(e => e.SourceBytes).HasColumnName("source_bytes");
            entity.Property(e => e.SourceMime).HasColumnName("source_mime");
            entity.Property(e => e.State).HasColumnName("state");
            entity.Property(e => e.Width).HasColumnName("width");
        });

        modelBuilder.Entity<MediaModerationAudit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("media_moderation_audits_pkey");

            entity.ToTable("media_moderation_audits");

            entity.HasIndex(e => new { e.MediaId, e.CheckedAt }, "idx_media_moderation_media").IsDescending(false, true);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CheckedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("checked_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("now()")
                .HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.MediaId).HasColumnName("media_id");
            entity.Property(e => e.PolicyVersion).HasColumnName("policy_version");
            entity.Property(e => e.RawJson)
                .HasColumnType("jsonb")
                .HasColumnName("raw_json");
            entity.Property(e => e.Score)
                .HasPrecision(5, 4)
                .HasColumnName("score");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Media).WithMany(p => p.MediaModerationAudits)
                .HasForeignKey(d => d.MediaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_media_moderation_media");
        });

        modelBuilder.Entity<MediaOwner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("media_owners_pkey");

            entity.ToTable("media_owners");

            entity.HasIndex(e => e.MediaId, "idx_media_owners_media");

            entity.HasIndex(e => new { e.MediaOwnerType, e.MediaOwnerId }, "idx_media_owners_owner");

            entity.HasIndex(e => new { e.MediaId, e.MediaOwnerType, e.MediaOwnerId }, "uq_media_owner_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("now()")
                .HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.MediaId).HasColumnName("media_id");
            entity.Property(e => e.MediaOwnerId).HasColumnName("media_owner_id");
            entity.Property(e => e.MediaOwnerType).HasColumnName("media_owner_type");

            entity.HasOne(d => d.Media).WithMany(p => p.MediaOwners)
                .HasForeignKey(d => d.MediaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_media_owners_media");
        });

        modelBuilder.Entity<MediaProcessingJob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("media_processing_jobs_pkey");

            entity.ToTable("media_processing_jobs");

            entity.HasIndex(e => e.MediaId, "idx_media_jobs_media");

            entity.HasIndex(e => e.NextRetryAt, "idx_media_jobs_next_retry");

            entity.HasIndex(e => e.Status, "idx_media_jobs_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Attempt)
                .HasDefaultValue(0)
                .HasColumnName("attempt");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.JobType).HasColumnName("job_type");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("now()")
                .HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.MediaId).HasColumnName("media_id");
            entity.Property(e => e.NextRetryAt).HasColumnName("next_retry_at");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Media).WithMany(p => p.MediaProcessingJobs)
                .HasForeignKey(d => d.MediaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_media_jobs_media");
        });

        modelBuilder.Entity<MediaVariant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("media_variants_pkey");

            entity.ToTable("media_variants");

            entity.HasIndex(e => new { e.VariantType, e.Format }, "idx_media_variants_kind");

            entity.HasIndex(e => e.MediaId, "idx_media_variants_media");

            entity.HasIndex(e => new { e.MediaId, e.VariantType, e.Format }, "uq_media_variant_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BucketKey).HasColumnName("bucket_key");
            entity.Property(e => e.Bytes).HasColumnName("bytes");
            entity.Property(e => e.CdnUrl).HasColumnName("cdn_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Format).HasColumnName("format");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("now()")
                .HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.MediaId).HasColumnName("media_id");
            entity.Property(e => e.VariantType).HasColumnName("variant_type");
            entity.Property(e => e.Width).HasColumnName("width");

            entity.HasOne(d => d.Media).WithMany(p => p.MediaVariants)
                .HasForeignKey(d => d.MediaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_media_variants_media");
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("outbox_messages_pkey");

            entity.ToTable("outbox_messages");

            entity.HasIndex(e => e.AggregateId, "idx_outbox_aggregate");

            entity.HasIndex(e => e.Type, "idx_outbox_type");

            entity.HasIndex(e => e.ProcessedAt, "idx_outbox_unprocessed").HasFilter("(processed_at IS NULL)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AggregateId).HasColumnName("aggregate_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedAtActor).HasColumnName("created_at_actor");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("now()")
                .HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.Payload)
                .HasColumnType("jsonb")
                .HasColumnName("payload");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.Type).HasColumnName("type");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
