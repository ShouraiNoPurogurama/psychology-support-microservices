using Media.Application.Data;
using Media.Domain.Enums;
using Media.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Media.Infrastructure.Data;

public class MediaDbContext : DbContext, IMediaDbContext
{
    public MediaDbContext(DbContextOptions<MediaDbContext> options) : base(options)
    {
    }

    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<MediaModerationAudit> MediaModerationAudits => Set<MediaModerationAudit>();
    public DbSet<MediaOwner> MediaOwners => Set<MediaOwner>();
    public DbSet<MediaProcessingJob> MediaProcessingJobs => Set<MediaProcessingJob>();
    public DbSet<MediaVariant> MediaVariants => Set<MediaVariant>();

    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("public");

        ApplyEntityConfigurations(modelBuilder);
    }


    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }


    private void ApplyEntityConfigurations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Id).ValueGeneratedNever();

            entity.OwnsOne(m => m.Content, content =>
            {
                content.Property(c => c.MimeType).HasColumnName("mime_type").IsRequired().HasMaxLength(100);
                content.Property(c => c.SizeInBytes).HasColumnName("size_in_bytes").IsRequired();
                content.Property(c => c.Phash64).HasColumnName("phash64").HasMaxLength(100);
            });

            entity.OwnsOne(m => m.Checksum, checksum =>
            {
                checksum.Property(c => c.Value).HasColumnName("checksum_value").IsRequired().HasMaxLength(100);
                checksum.Property(c => c.Algorithm).HasColumnName("checksum_algorithm").IsRequired().HasMaxLength(50);
            });

            entity.OwnsOne(m => m.Dimensions, dimensions =>
            {
                dimensions.Property(d => d.Width).HasColumnName("width");
                dimensions.Property(d => d.Height).HasColumnName("height");
                dimensions.Property(d => d.AspectRatioNumerator).HasColumnName("aspect_ratio_numerator");
                dimensions.Property(d => d.AspectRatioDenominator).HasColumnName("aspect_ratio_denominator");
            });

            entity.OwnsOne(m => m.Moderation, moderation =>
            {
                moderation.Property(mod => mod.Status)
                    .HasColumnName("moderation_status")
                    .HasConversion(s => s.ToString(),
                        dbStatus => (MediaModerationStatus)Enum.Parse(typeof(MediaModerationStatus), dbStatus))
                    .IsRequired()
                    .HasMaxLength(50);
                moderation.Property(mod => mod.Score).HasColumnName("moderation_score").HasPrecision(5, 4);
                moderation.Property(mod => mod.CheckedAt).HasColumnName("moderation_checked_at");
                moderation.Property(mod => mod.PolicyVersion).HasColumnName("moderation_policy_version").HasMaxLength(50);
                moderation.Property(mod => mod.RawJson).HasColumnName("moderation_raw_json").HasColumnType("text");
            });

            entity.Property(m => m.State)
                .HasConversion(s => s.ToString(),
                    dbState => (MediaState)Enum.Parse(typeof(MediaState), dbState))
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(m => m.ExifRemoved).HasColumnName("exif_removed").HasDefaultValue(false);
            entity.Property(m => m.HoldThumbUntilPass).HasColumnName("hold_thumb_until_pass").HasDefaultValue(false);

            entity.Property(m => m.Purpose)
                .HasConversion(p => p.ToString(),
                    dbPurpose => (MediaPurpose)Enum.Parse(typeof(MediaPurpose), dbPurpose))
                .HasDefaultValue(MediaPurpose.NotSpecified)
                .HasSentinel(MediaPurpose.NotSpecified);
            
            entity.Property(m => m.DeletedAt).HasColumnName("deleted_at");
            entity.Property(m => m.DeletedBy).HasColumnName("deleted_by").HasMaxLength(255);

            entity.HasIndex(m => m.State)
                .HasDatabaseName("ix_media_assets_state");

            entity.HasIndex(m => new { m.CreatedAt, m.State })
                .HasDatabaseName("ix_media_assets_created_state");
        });


        modelBuilder.Entity<MediaModerationAudit>(entity =>
        {
            entity.HasKey(ma => ma.Id);
            entity.Property(ma => ma.Id).ValueGeneratedNever();

            entity.Property(ma => ma.Status)
                .HasConversion(s => s.ToString(),
                    dbStatus => (MediaModerationStatus)Enum.Parse(typeof(MediaModerationStatus), dbStatus))
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(ma => ma.Score).HasPrecision(5, 4);

            entity.Property(ma => ma.RawJson).HasColumnType("text");

            entity.Property(ma => ma.PolicyVersion).HasMaxLength(50);

            entity.HasOne(ma => ma.Media)
                .WithMany(m => m.ModerationAudits)
                .HasForeignKey(ma => ma.MediaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(ma => ma.MediaId)
                .HasDatabaseName("ix_moderation_audits_media_id");

            entity.HasIndex(ma => ma.CheckedAt)
                .HasDatabaseName("ix_moderation_audits_checked_at");
        });


        modelBuilder.Entity<MediaOwner>(entity =>
        {
            entity.HasKey(mo => mo.Id);
            entity.Property(mo => mo.Id).ValueGeneratedNever();

            entity.Property(mo => mo.MediaOwnerType)
                .HasConversion(mot => mot.ToString(),
                    dbOwnerType => (MediaOwnerType)Enum.Parse(typeof(MediaOwnerType), dbOwnerType))
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(mo => mo.Media)
                .WithMany(m => m.Owners)
                .HasForeignKey(mo => mo.MediaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(mo => new { mo.MediaId, mo.MediaOwnerType, mo.MediaOwnerId })
                .IsUnique()
                .HasDatabaseName("ux_media_owners_unique");

            entity.HasIndex(mo => new { mo.MediaOwnerType, mo.MediaOwnerId })
                .HasDatabaseName("ix_media_owners_owner");
        });


        modelBuilder.Entity<MediaProcessingJob>(entity =>
        {
            entity.HasKey(mpj => mpj.Id);
            entity.Property(mpj => mpj.Id).ValueGeneratedNever();

            entity.Property(mpj => mpj.JobType)
                .HasConversion(jt => jt.ToString(),
                    dbJobType => (JobType)Enum.Parse(typeof(JobType), dbJobType))
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(mpj => mpj.Status)
                .HasConversion(ps => ps.ToString(),
                    dbStatus => (ProcessStatus)Enum.Parse(typeof(ProcessStatus), dbStatus))
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(mpj => mpj.ErrorMessage).HasColumnType("text");

            entity.Property(mpj => mpj.Attempt).HasDefaultValue(0);

            entity.HasOne(mpj => mpj.Media)
                .WithMany(m => m.ProcessingJobs)
                .HasForeignKey(mpj => mpj.MediaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(mpj => mpj.MediaId)
                .HasDatabaseName("ix_processing_jobs_media_id");

            entity.HasIndex(mpj => new { mpj.Status, mpj.NextRetryAt })
                .HasDatabaseName("ix_processing_jobs_queue")
                .HasFilter($"status NOT IN ('{nameof(ProcessStatus.Succeeded)}', '{nameof(ProcessStatus.Failed)}', '{nameof(ProcessStatus.Cancelled)}')"); 

            entity.HasIndex(mpj => new { mpj.JobType, mpj.Status })
                .HasDatabaseName("ix_processing_jobs_type_status");
        });

        // =========================
        // MediaVariant
        // =========================
        modelBuilder.Entity<MediaVariant>(entity =>
        {
            entity.HasKey(mv => mv.Id);
            entity.Property(mv => mv.Id).ValueGeneratedNever();

            // Enum conversions
            entity.Property(mv => mv.VariantType)
                .HasConversion(vt => vt.ToString(),
                    dbVariantType => (VariantType)Enum.Parse(typeof(VariantType), dbVariantType))
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(mv => mv.Format)
                .HasConversion(f => f.ToString(),
                    dbFormat => (MediaFormat)Enum.Parse(typeof(MediaFormat), dbFormat))
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(mv => mv.BucketKey).IsRequired().HasMaxLength(500);
            entity.Property(mv => mv.CdnUrl).HasMaxLength(1000);

            entity.HasOne(mv => mv.Media)
                .WithMany(m => m.Variants)
                .HasForeignKey(mv => mv.MediaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(mv => new { mv.MediaId, mv.VariantType })
                .IsUnique()
                .HasDatabaseName("ux_media_variants_media_type");

            entity.HasIndex(mv => mv.MediaId)
                .HasDatabaseName("ix_media_variants_media_id");

            entity.HasIndex(mv => mv.CdnUrl)
                .HasDatabaseName("ix_media_variants_cdn_url")
                .HasFilter("cdn_url IS NOT NULL");
        });


        modelBuilder.Entity<IdempotencyKey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idempotency_keys_pkey");
            entity.ToTable("idempotency_keys");

            entity.HasIndex(e => e.Key, "idempotency_keys_key").IsUnique();
            entity.HasIndex(e => e.ExpiresAt, "ix_idempotency_keys_expires_at");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.RequestHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ResponsePayload).HasColumnType("text");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("outbox_messages_pkey");
            entity.ToTable("outbox_messages");

            entity.HasIndex(e => e.OccurredOn, "ix_outbox_pending")
                .HasFilter("processed_on IS NULL");
            entity.HasIndex(e => e.ProcessedOn, "ix_outbox_processed")
                .HasFilter("processed_on IS NOT NULL");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Payload).IsRequired().HasColumnType("text");
        });
    }
}