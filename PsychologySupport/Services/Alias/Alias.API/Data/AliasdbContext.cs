using Alias.API.Enums;
using Alias.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Alias.API.Data;

public partial class AliasdbContext : DbContext
{
    public AliasdbContext()
    {
    }

    public AliasdbContext(DbContextOptions<AliasdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Models.Alias> Aliases { get; set; }

    public virtual DbSet<AliasAudit> AliasAudits { get; set; }

    public virtual DbSet<AliasOwnerMap> AliasOwnerMaps { get; set; }

    public virtual DbSet<AliasVersion> AliasVersions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<NicknameSource>("public", "nickname_source")
            .HasPostgresExtension("citext");

        modelBuilder.Entity<Models.Alias>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("aliases_pkey");

            entity.ToTable("aliases");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CurrentVersionId).HasColumnName("current_version_id");
            entity.Property(e => e.LastModified).HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
        });

        modelBuilder.Entity<AliasAudit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("alias_audits_pkey");

            entity.ToTable("alias_audits");

            entity.HasIndex(e => e.AliasId, "ix_alias_audits_alias");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Action).HasColumnName("action");
            entity.Property(e => e.AliasId).HasColumnName("alias_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Details)
                .HasColumnType("jsonb")
                .HasColumnName("details");
            entity.Property(e => e.LastModified).HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
        });

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
            entity.Property(e => e.LastModified).HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<AliasVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("alias_versions_pkey");

            entity.ToTable("alias_versions");

            entity.HasIndex(e => e.AliasKey, "uniq_alias_key_current")
                .IsUnique()
                .HasFilter("(valid_to IS NULL)");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AliasId).HasColumnName("alias_id");
            entity.Property(e => e.AliasKey)
                .HasColumnType("citext")
                .HasColumnName("alias_key");
            entity.Property(e => e.AliasLabel).HasColumnName("alias_label");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.LastModified).HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
            entity.Property(e => e.ValidTo).HasColumnName("valid_to");

            entity.HasOne(d => d.Alias).WithMany(p => p.AliasVersions)
                .HasForeignKey(d => d.AliasId)
                .HasConstraintName("alias_versions_alias_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
