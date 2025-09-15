using Media.Application.Data;
using Media.Domain.Enums;
using Media.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Media.Infrastructure.Data;

public partial class MediaDbContext : DbContext, IMediaDbContext
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

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        => base.SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);

        modelBuilder.Entity<MediaAsset>()
          .Property(e => e.State)
          .HasConversion(new EnumToStringConverter<MediaState>())
          .HasColumnType("VARCHAR(20)");

        modelBuilder.Entity<MediaModerationAudit>()
          .Property(e => e.Status)
          .HasConversion(new EnumToStringConverter<MediaModerationStatus>())
          .HasColumnType("VARCHAR(20)");

        modelBuilder.Entity<MediaOwner>()
         .Property(e => e.MediaOwnerType)
         .HasConversion(new EnumToStringConverter<MediaOwnerType>())
         .HasColumnType("VARCHAR(20)");

        modelBuilder.Entity<MediaProcessingJob>()
         .Property(e => e.JobType)
         .HasConversion(new EnumToStringConverter<JobType>())
         .HasColumnType("VARCHAR(20)");

        modelBuilder.Entity<MediaProcessingJob>()
          .Property(e => e.Status)
          .HasConversion(new EnumToStringConverter<ProcessStatus>())
          .HasColumnType("VARCHAR(20)");

        modelBuilder.Entity<MediaProcessingJob>()
          .Property(e => e.Status)
          .HasConversion(new EnumToStringConverter<ProcessStatus>())
          .HasColumnType("VARCHAR(20)");

        modelBuilder.Entity<MediaVariant>()
         .Property(e => e.VariantType)
         .HasConversion(new EnumToStringConverter<VariantType>())
         .HasColumnType("VARCHAR(20)");

        modelBuilder.Entity<MediaVariant>()
         .Property(e => e.Format)
         .HasConversion(new EnumToStringConverter<MediaFormat>())
         .HasColumnType("VARCHAR(20)");

    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
