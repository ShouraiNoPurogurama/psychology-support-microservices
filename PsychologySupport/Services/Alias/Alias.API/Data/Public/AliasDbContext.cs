using Alias.API.Aliases.Models;
using Alias.API.Aliases.Models.Enums;

namespace Alias.API.Data.Public;

using Alias = Aliases.Models.Alias;

public partial class AliasDbContext : DbContext
{
    public AliasDbContext(DbContextOptions<AliasDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alias> Aliases { get; set; }

    public virtual DbSet<AliasAudit> AliasAudits { get; set; }

    public virtual DbSet<AliasVersion> AliasVersions { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // modelBuilder
        //     .HasDefaultSchema("public")
        //     .HasPostgresEnum<NicknameSource>("public", "nickname_source")
        //     .HasPostgresExtension("citext");

        modelBuilder.Entity<Alias>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("aliases_pkey");

            entity.ToTable("aliases");

            entity.Property(e => e.Id)
                .ValueGeneratedNever();
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.CreatedBy);
            entity.Property(e => e.CurrentVersionId);
            entity.Property(e => e.LastModified);
            entity.Property(e => e.LastModifiedBy);

            entity.Property(e => e.Visibility)
                .HasDefaultValue(AliasVisibility.Public)
                .HasSentinel(AliasVisibility.Public)
                .HasConversion(s => s.ToString(),
                    dbStatus => (AliasVisibility)Enum.Parse(typeof(AliasVisibility), dbStatus));
            
            // Map the AliasLabel value object as an owned type with explicit column names
            entity.OwnsOne(a => a.Label, label =>
            {
                label.Property(l => l.Value).HasColumnName("value");
                label.Property(l => l.SearchKey).HasColumnName("search_key");
                label.Property(l => l.UniqueKey).HasColumnName("unique_key");
            });

            // Map the AliasMetadata value object as an owned type with explicit column names
            entity.OwnsOne(a => a.Metadata, metadata =>
            {
                metadata.Property(m => m.IsSystemGenerated).HasColumnName("is_system_generated");
                metadata.Property(m => m.LastActiveAt).HasColumnName("last_active_at");
                metadata.Property(m => m.VersionCount).HasColumnName("version_count");
            });

            entity.HasMany(a => a.Versions)
                .WithOne()
                .HasForeignKey(av => av.AliasId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AliasAudit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("alias_audits_pkey");

            entity.ToTable("alias_audits");

            entity.HasIndex(e => e.AliasId, "ix_alias_audits_alias");

            entity.Property(e => e.Id)
                .ValueGeneratedNever();
            
            entity.Property(e => e.Details)
                .HasColumnType("jsonb");
        });

        modelBuilder.Entity<AliasVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("alias_versions_pkey");

            entity.ToTable("alias_versions");

            entity.HasIndex(e => e.SearchKey, "idx_search_key_current")
                .HasFilter("(valid_to IS NULL)");
            
            entity.HasIndex(e => e.UniqueKey, "uniq_unique_key_current")
                .IsUnique()
                .HasFilter("(valid_to IS NULL)");

            entity.Property(e => e.SearchKey)
                .HasColumnType("citext");

            entity.Property(e => e.UniqueKey)
                .HasColumnType("citext");

            entity.Property(e => e.DisplayName);

            entity.Property(e => e.NicknameSource)
                .HasConversion(s => s.ToString(),
                    dbStatus => (NicknameSource)Enum.Parse(typeof(NicknameSource), dbStatus));
            
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.CreatedBy);
            entity.Property(e => e.ValidFrom);
            entity.Property(e => e.ValidTo);
            

            entity.HasIndex(e => e.AliasId, "ix_alias_versions_alias_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}