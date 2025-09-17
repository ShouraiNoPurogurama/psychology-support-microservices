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
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CurrentVersionId).HasColumnName("current_version_id");
            entity.Property(e => e.LastModified).HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");

            entity.Property(e => e.Visibility)
                .HasDefaultValue(AliasVisibility.Public)
                .HasSentinel(AliasVisibility.Public)
                .HasConversion(s => s.ToString(),
                    dbStatus => (AliasVisibility)Enum.Parse(typeof(AliasVisibility), dbStatus));

            entity.HasOne<AliasVersion>()
                .WithMany()
                .HasForeignKey(e => e.CurrentVersionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("aliases_current_version_id_fkey");
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

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity
                .Property(e => e.AliasId)
                .HasColumnName("alias_id");

            entity.Property(e => e.SearchKey)
                .HasColumnType("citext");

            entity.Property(e => e.UniqueKey)
                .HasColumnType("citext");
            
            entity.Property(e => e.Label).HasColumnName("alias_label");

            entity.Property(e => e.NicknameSource)
                .HasColumnName("nickname_source")
                .HasConversion(s => s.ToString(),
                    dbStatus => (NicknameSource)Enum.Parse(typeof(NicknameSource), dbStatus));

            // entity.Property(e => e.NicknameSource)
            //     .HasColumnType("public.nickname_source")
            //     .HasColumnName("nickname_source");

            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.ValidFrom).HasColumnName("valid_from");
            entity.Property(e => e.ValidTo).HasColumnName("valid_to");

            modelBuilder.Entity<Alias>()
                .HasOne(a => a.CurrentVersion)
                .WithOne()
                .HasForeignKey<Alias>(a => a.CurrentVersionId)
                .OnDelete(DeleteBehavior.SetNull); // Đảm bảo xử lý khi phiên bản bị xóa

            entity.HasIndex(e => e.AliasId, "ix_alias_versions_alias_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}