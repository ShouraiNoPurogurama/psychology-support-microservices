using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Post.Application.Data;
using Post.Domain.Aggregates.CategoryTags;
using Post.Domain.Aggregates.Comments;
using Post.Domain.Aggregates.Gifts;
using Post.Domain.Aggregates.Posts;
using Post.Domain.Aggregates.Posts.ValueObjects;
using Post.Domain.Aggregates.Reaction;
using Post.Domain.Enums;
using Post.Infrastructure.Integration.Entities;
using Post.Infrastructure.Resilience.Entities;

namespace Post.Infrastructure.Data.Post;

using Post = Domain.Aggregates.Posts.Post;

public class PostDbContext : DbContext, IPostDbContext
{
    public PostDbContext(DbContextOptions<PostDbContext> options) : base(options)
    {
    }

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostMedia> PostMedia => Set<PostMedia>();
    public DbSet<PostCategory> PostCategories => Set<PostCategory>();
    public DbSet<PostEmotion> PostEmotions => Set<PostEmotion>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<CategoryTag> CategoryTags => Set<CategoryTag>();
    public DbSet<GiftAttach> GiftAttaches => Set<GiftAttach>();

    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure database schema following project standard
        modelBuilder.HasDefaultSchema("post");

        // Apply entity configurations for all aggregates
        ApplyEntityConfigurations(modelBuilder);
    }


    /// <summary>
    /// Begin database transaction for complex operations
    /// Supporting transactional operations across aggregates
    /// </summary>
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Apply entity configurations for all Post domain aggregates
    /// Based on actual domain value objects and entities
    /// </summary>
    private void ApplyEntityConfigurations(ModelBuilder modelBuilder)
    {
        // =========================
        // Post (Aggregate Root)
        // =========================
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedNever();

            // PostContent (owned)
            entity.OwnsOne(p => p.Content, content =>
            {
                content.Property(c => c.Value).HasColumnName("content").IsRequired().HasMaxLength(10000);
                content.Property(c => c.Title).HasColumnName("title").HasMaxLength(200);
                content.Property(c => c.WordCount).HasColumnName("word_count");
                content.Property(c => c.CharacterCount).HasColumnName("character_count");
            });

            // AuthorInfo (owned)
            entity.OwnsOne(p => p.Author, author =>
            {
                author.Property(a => a.AliasId).HasColumnName("author_alias_id").IsRequired();
                author.Property(a => a.AliasVersionId).HasColumnName("author_alias_version_id");
            });

            // ModerationInfo (owned)
            entity.OwnsOne(p => p.Moderation, moderation =>
            {
                moderation.Property(m => m.Status).HasColumnName("moderation_status").HasConversion<string>();
                moderation.Property(m => m.PolicyVersion).HasColumnName("policy_version");
                moderation.Property(m => m.ModeratedAt).HasColumnName("moderated_at");

                moderation.Property(m => m.Reasons)
                    .HasColumnName("moderation_reasons")
                    .HasConversion(
                        v => string.Join(";", v),
                        v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList().AsReadOnly(),
                        new ValueComparer<IReadOnlyList<string>>(
                            (c1, c2) => c1.SequenceEqual(c2),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.ToList()
                        )
                    );
            });

            // PostMetrics (owned)
            entity.OwnsOne(p => p.Metrics, metrics =>
            {
                metrics.Property(m => m.ReactionCount).HasColumnName("reaction_count");
                metrics.Property(m => m.CommentCount).HasColumnName("comment_count");
                metrics.Property(m => m.ShareCount).HasColumnName("share_count");
                metrics.Property(m => m.ViewCount).HasColumnName("view_count");
            });

            // Enum
            entity.Property(p => p.Visibility)
                .HasConversion
                (pv => pv.ToString(), str =>
                    (PostVisibility)Enum.Parse(typeof(PostVisibility), str));

            // index 1: visibility + created_at
            entity.HasIndex(
                    nameof(Post.Visibility),
                    nameof(Post.CreatedAt)
                )
                .HasDatabaseName("ix_posts_feed_vis_created");

            // index 2: moderation_status (owned) + created_at
            // CHÚ Ý: đặt index trên owned builder, vì property thuộc owned type
            entity.OwnsOne(p => p.Moderation, builder =>
                {
                    builder.Property(b => b.Status).HasColumnName("moderation_status").HasConversion<string>();
                    builder.Property(b => b.ModeratedAt).HasColumnName("moderated_at");
                    builder.Property(b => b.PolicyVersion).HasColumnName("policy_version");
                    builder.Property(b => b.ModeratedAt).HasColumnName("moderated_at");
                })
                .HasIndex(nameof(ModerationInfo.Status), nameof(ModerationInfo.ModeratedAt)) // nếu muốn queue duyệt
                .HasDatabaseName("ix_posts_feed_mod_status"); // hoặc chỉ Status, tùy use-case
        });

        // =========================
        // PostMedia
        // =========================
        modelBuilder.Entity<PostMedia>(entity =>
        {
            entity.HasKey(pm => pm.Id);
            entity.Property(pm => pm.Id).ValueGeneratedNever();

            // Các cột đơn giản để convention tự snake_case
            entity.Property(pm => pm.Caption).HasMaxLength(500);
            entity.Property(pm => pm.AltText).HasMaxLength(500);

            modelBuilder.Entity<PostMedia>(entity =>
            {
                entity.HasIndex(pm => pm.PostId)
                    .HasDatabaseName("ix_post_media_post_id");
            });
        });

        // =========================
        // PostCategory
        // =========================
        modelBuilder.Entity<PostCategory>(entity =>
        {
            entity.HasKey(pc => pc.Id);
            entity.Property(pc => pc.Id).ValueGeneratedNever();

            // Lấy category của 1 post
            entity.HasIndex(pc => pc.PostId)
                .HasDatabaseName("ix_post_categories_post_id");

            // Lấy posts theo category
            entity.HasIndex(pc => pc.CategoryTagId)
                .HasDatabaseName("ix_post_categories_category_tag_id");
        });

        // =========================
        // PostEmotion
        // =========================
        modelBuilder.Entity<PostEmotion>(entity =>
        {
            entity.HasKey(pe => pe.Id);
            entity.Property(pe => pe.Id).ValueGeneratedNever();

            // Precision cho confidence là cấu hình đặc thù
            entity.Property(pe => pe.Confidence).HasPrecision(3, 2);
        });

        // =========================
        // Comment (Aggregate Root)
        // =========================
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).ValueGeneratedNever();

            // CommentContent (owned)
            entity.OwnsOne(c => c.Content, content =>
            {
                content.Property(co => co.Value).HasColumnName("content").IsRequired().HasMaxLength(2000);
                content.Property(co => co.CharacterCount).HasColumnName("character_count");
                content.Property(co => co.WordCount).HasColumnName("word_count");
            });

            // CommentHierarchy (owned)
            entity.OwnsOne(c => c.Hierarchy, hierarchy =>
            {
                hierarchy.Property(h => h.ParentCommentId).HasColumnName("parent_comment_id");
                hierarchy.Property(h => h.Path).HasColumnName("path").HasMaxLength(1000);
                hierarchy.Property(h => h.Level).HasColumnName("level");
            });

            // AuthorInfo (owned)
            entity.OwnsOne(c => c.Author, author =>
            {
                author.Property(a => a.AliasId).HasColumnName("author_alias_id").IsRequired();
                author.Property(a => a.AliasVersionId).HasColumnName("author_alias_version_id");
            });

            // ModerationInfo (owned)
            entity.OwnsOne(c => c.Moderation, moderation =>
            {
                moderation.Property(m => m.Status).HasColumnName("moderation_status").HasConversion<string>();
                moderation.Property(m => m.PolicyVersion).HasColumnName("policy_version");
                moderation.Property(m => m.ModeratedAt).HasColumnName("moderated_at");
                moderation.Property(m => m.Reasons)
                    .HasColumnName("moderation_reasons")
                    .HasConversion(
                        v => string.Join(";", v),
                        v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList().AsReadOnly()
                    );
            });

            //List comment theo post (paging thời gian).
            entity.HasIndex(c => new { c.PostId, c.CreatedAt })
                .HasDatabaseName("ix_comments_post_created");
            
        });

        // =========================
        // Reaction (Aggregate Root)
        // =========================
        modelBuilder.Entity<Reaction>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).ValueGeneratedNever();

            // ReactionTarget (owned)
            entity.OwnsOne(r => r.Target, target =>
            {
                target.Property(t => t.TargetType).HasColumnName("target_type").IsRequired().HasMaxLength(50);
                target.Property(t => t.TargetId).HasColumnName("target_id").IsRequired();
            });

            // ReactionType (owned)
            entity.OwnsOne(r => r.Type, type =>
            {
                type.Property(t => t.Code).HasColumnName("reaction_code").IsRequired().HasMaxLength(50);
                type.Property(t => t.Emoji).HasColumnName("reaction_emoji").HasMaxLength(10);
                type.Property(t => t.Weight).HasColumnName("reaction_weight");
            });

            // AuthorInfo (owned)
            entity.OwnsOne(r => r.Author, author =>
            {
                author.Property(a => a.AliasId).HasColumnName("author_alias_id").IsRequired();
                author.Property(a => a.AliasVersionId).HasColumnName("author_alias_version_id");
            });
            
        });

        // PostEmotion entity configuration
        modelBuilder.Entity<PostEmotion>(entity =>
        {
            entity.HasKey(pe => pe.Id);
            entity.Property(pe => pe.Id).ValueGeneratedNever();

            entity.Property(pe => pe.Confidence)
                .HasPrecision(3, 2)
                .HasDefaultValue(1.0);

            entity.Property(pe => pe.IsDeleted).HasDefaultValue(false);

            entity.HasIndex(pe => pe.PostId).HasDatabaseName("ix_post_emotions_post_id");
        });


        modelBuilder.Entity<PostCounterDelta>(entity =>
        {
            entity.HasKey(pcd => pcd.Id);
            entity.Property(pcd => pcd.Id).ValueGeneratedNever();
            entity.Property(pcd => pcd.IsProcessed).HasDefaultValue(false);

            //Hàng đợi (partial) — chỉ index phần chưa xử lý
            entity.HasIndex(pcd => pcd.OccurredAt)
                .HasDatabaseName("ix_pcd_pending_time")
                .HasFilter("(is_processed = false)");

            //Tăng tốc projector gộp
            entity.HasIndex(pcd => new { pcd.PostId, pcd.CounterType, pcd.OccurredAt })
                .HasDatabaseName("ix_pcd_post_kind_time");
        });


        // CategoryTag Aggregate configuration - master data for post categorization
        modelBuilder.Entity<CategoryTag>(entity =>
        {
            entity.HasKey(ct => ct.Id);
            entity.Property(ct => ct.Id).ValueGeneratedNever();

            entity.Property(ct => ct.IsActive).HasDefaultValue(true);
            entity.Property(ct => ct.SortOrder).HasDefaultValue(0);

            entity.HasIndex(ct => ct.Code)
                .IsUnique()
                .HasDatabaseName("ux_category_tags_code");

        });

        modelBuilder.Entity<GiftAttach>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Id).ValueGeneratedNever();

            // Owned value objects (complex types) - giữ naming mặc định snake_case
            entity.OwnsOne(g => g.Target, target =>
            {
                target.Property(t => t.TargetType).HasColumnName("target_type").IsRequired().HasMaxLength(50);
                target.Property(t => t.TargetId).HasColumnName("target_id").IsRequired();
            });

            entity.OwnsOne(g => g.Info, info =>
            {
                info.Property(i => i.GiftId).HasColumnName("gift_id").IsRequired();
            });

            entity.OwnsOne(g => g.Sender, sender =>
            {
                sender.Property(s => s.AliasId).HasColumnName("sender_alias_id").IsRequired();
                sender.Property(s => s.AliasVersionId).HasColumnName("sender_alias_version_id");
            });

            // Scalars
            entity.Property(g => g.Message).HasMaxLength(500);
            entity.Property(g => g.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<IdempotencyKey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idempotency_keys_pkey");
            entity.ToTable("idempotency_keys");

            entity.HasIndex(e => e.Key, "idempotency_keys_idempotency_key").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("outbox_messages_pkey");
            entity.ToTable("outbox_messages");

            entity.HasIndex(e => e.OccurredOn, "ix_outbox_pending")
                .HasFilter("(processed_on IS NULL)");
            entity.HasIndex(e => e.ProcessedOn, "ix_outbox_processed")
                .HasFilter("(processed_on IS NOT NULL)");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });
    }
}