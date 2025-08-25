using System;
using System.Collections.Generic;
using Alias.API.Models.Pii;
using Microsoft.EntityFrameworkCore;

namespace Alias.API.Data.Pii;

public partial class PiiDbContext : DbContext
{
    public PiiDbContext(DbContextOptions<PiiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AliasOwnerMap> AliasOwnerMaps { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AliasOwnerMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("alias_owner_map_pkey");

            entity.ToTable("alias_owner_map", "pii");

            entity.HasIndex(e => e.AliasId, "alias_owner_map_alias_id_key").IsUnique();

            entity.HasIndex(e => e.UserId, "alias_owner_map_user_id_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AliasId).HasColumnName("alias_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
