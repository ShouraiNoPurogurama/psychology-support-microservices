using Media.API.Media.Models;
using Media.Application.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
