using Alias.API.Aliases.Models;
using Alias.API.Aliases.Models.Aliases;
using Alias.API.Aliases.Models.Aliases.Enums;
using Alias.API.Aliases.Models.Follows;

namespace Alias.API.Data.Public;

using Alias = Aliases.Models.Aliases.Alias;
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTimeOffset OccurredOn { get; set; }
    public DateTimeOffset? ProcessedOn { get; set; }
}


public partial class AliasDbContext : DbContext
{
    public AliasDbContext(DbContextOptions<AliasDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alias> Aliases { get; set; }

    public virtual DbSet<AliasAudit> AliasAudits { get; set; }

    public virtual DbSet<AliasVersion> AliasVersions { get; set; }

    public virtual DbSet<Follow> Follows { get; set; }

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

            entity.Property(e => e.Visibility)
                .HasDefaultValue(AliasVisibility.Public)
                .HasSentinel(AliasVisibility.Public)
                .HasConversion(s => s.ToString(),
                    dbStatus => (AliasVisibility)Enum.Parse(typeof(AliasVisibility), dbStatus));

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
                metadata.Property(m => m.FollowersCount).HasColumnName("followers_count");
                metadata.Property(m => m.FollowingCount).HasColumnName("following_count");
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

            entity.Property(e => e.Action)
                .HasConversion(a => a.ToString(),
                    dbAction => (AliasAuditAction)Enum.Parse(typeof(AliasAuditAction), dbAction));
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

        modelBuilder.Entity<Follow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("follows_pkey");

            entity.ToTable("follows");

            // Composite unique index to prevent duplicate follow relationships
            entity.HasIndex(e => new { e.FollowerAliasId, e.FollowedAliasId })
                .IsUnique()
                .HasDatabaseName("uix_follows_follower_followed");

            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.FollowedAt)
                .HasColumnName("followed_at");

            entity.Property(e => e.FollowerAliasId)
                .HasColumnName("follower_alias_id");

            entity.Property(e => e.FollowedAliasId)
                .HasColumnName("followed_alias_id");

            // Indexes for query performance
            entity.HasIndex(e => e.FollowerAliasId, "ix_follows_follower_alias_id");
            entity.HasIndex(e => e.FollowedAliasId, "ix_follows_followed_alias_id");

            // Foreign key relationships with cascade delete
            entity.HasOne<Alias>()
                .WithMany()
                .HasForeignKey(f => f.FollowerAliasId)
                .HasConstraintName("fk_follows_follower_alias")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Alias>()
                .WithMany()
                .HasForeignKey(f => f.FollowedAliasId)
                .HasConstraintName("fk_follows_followed_alias")
                .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}