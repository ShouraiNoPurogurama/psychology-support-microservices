using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Domain.Models;

namespace Post.Infrastructure.Data;

public partial class PostDbContext : DbContext, IPostDbContext
{
    public PostDbContext(DbContextOptions<PostDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CategoryTag> CategoryTags { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<EmotionTag> EmotionTags { get; set; }

    public virtual DbSet<GiftsAttach> GiftsAttaches { get; set; }

    public virtual DbSet<IdempotencyKey> IdempotencyKeys { get; set; }

    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }

    public virtual DbSet<Domain.Models.Post> Posts { get; set; }

    public virtual DbSet<PostCategory> PostCategories { get; set; }

    public virtual DbSet<PostCounterDelta> PostCounterDeltas { get; set; }

    public virtual DbSet<PostEmotion> PostEmotions { get; set; }

    public virtual DbSet<PostMedium> PostMedia { get; set; }

    public virtual DbSet<Reaction> Reactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoryTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("category_tags_pkey");

            entity.ToTable("category_tags");

            entity.HasIndex(e => e.Code, "category_tags_code_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Color).HasColumnName("color");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastModified).HasColumnName("last_modified");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");
            entity.Property(e => e.UnicodeCodepoint).HasColumnName("unicode_codepoint");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("comments_pkey");

            entity.ToTable("comments");

            entity.HasIndex(e => new { e.PostId, e.CreatedAt, e.Id }, "ix_comments_post_created")
                .IsDescending(false, true, true)
                .HasFilter("((deleted_at IS NULL) AND (moderation_status = 'approved'::text))");

            entity.HasIndex(e => new { e.PostId, e.Path }, "ix_comments_post_path");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AuthorAliasId).HasColumnName("author_alias_id");
            entity.Property(e => e.AuthorAliasVersionId).HasColumnName("author_alias_version_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.LastModified).HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.Level).HasColumnName("level");
            entity.Property(e => e.ModerationStatus)
                .HasDefaultValueSql("'pending'::text")
                .HasColumnName("moderation_status");
            entity.Property(e => e.ParentCommentId).HasColumnName("parent_comment_id");
            entity.Property(e => e.Path).HasColumnName("path");
            entity.Property(e => e.PostId).HasColumnName("post_id");
        });

        modelBuilder.Entity<EmotionTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("emotion_tags_pkey");

            entity.ToTable("emotion_tags");

            entity.HasIndex(e => e.Code, "emotion_tags_code_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Color).HasColumnName("color");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DigitalGoodId).HasColumnName("digital_good_id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Icon).HasColumnName("icon");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastModified).HasColumnName("last_modified");
            entity.Property(e => e.MediaId).HasColumnName("media_id");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");
            entity.Property(e => e.Topic).HasColumnName("topic");
            entity.Property(e => e.UnicodeCodepoint).HasColumnName("unicode_codepoint");
        });

        modelBuilder.Entity<GiftsAttach>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("gifts_attach_pkey");

            entity.ToTable("gifts_attach");

            entity.HasIndex(e => new { e.TargetType, e.TargetId, e.CreatedAt }, "ix_gifts_target")
                .IsDescending(false, false, true)
                .HasFilter("(deleted_at IS NULL)");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.GiftId).HasColumnName("gift_id");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.SenderAliasId).HasColumnName("sender_alias_id");
            entity.Property(e => e.SenderAliasVersionId).HasColumnName("sender_alias_version_id");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.TargetType).HasColumnName("target_type");
        });

        modelBuilder.Entity<IdempotencyKey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idempotency_keys_pkey");

            entity.ToTable("idempotency_keys");

            entity.HasIndex(e => e.IdempotencyKey1, "idempotency_keys_idempotency_key_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IdempotencyKey1).HasColumnName("idempotency_key");
            entity.Property(e => e.RequestFingerprint).HasColumnName("request_fingerprint");
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("outbox_messages_pkey");

            entity.ToTable("outbox_messages");

            entity.HasIndex(e => e.OccuredOn, "ix_outbox_pending").HasFilter("(processed_on IS NULL)");

            entity.HasIndex(e => e.ProcessedOn, "ix_outbox_processed").HasFilter("(processed_on IS NOT NULL)");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.LastModified).HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.OccuredOn).HasColumnName("occured_on");
            entity.Property(e => e.ProcessedOn).HasColumnName("processed_on");
            entity.Property(e => e.Type).HasColumnName("type");
        });

        modelBuilder.Entity<Domain.Models.Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("posts_pkey");

            entity.ToTable("posts");

            entity.HasIndex(e => new { e.AuthorAliasId, e.CreatedAt }, "ix_posts_author")
                .IsDescending(false, true)
                .HasFilter("(deleted_at IS NULL)");

            entity.HasIndex(e => new { e.CreatedAt, e.Id }, "ix_posts_list")
                .IsDescending()
                .HasFilter("((deleted_at IS NULL) AND (moderation_status = 'approved'::text))");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AuthorAliasId).HasColumnName("author_alias_id");
            entity.Property(e => e.AuthorAliasVersionId).HasColumnName("author_alias_version_id");
            entity.Property(e => e.CommentCount)
                .HasDefaultValue(0)
                .HasColumnName("comment_count");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.LastModified).HasColumnName("last_modified");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.ModerationPolicyVersion).HasColumnName("moderation_policy_version");
            entity.Property(e => e.ModerationReasons)
                .HasDefaultValueSql("'{}'::text[]")
                .HasColumnName("moderation_reasons");
            entity.Property(e => e.ModerationStatus)
                .HasDefaultValueSql("'pending'::text")
                .HasColumnName("moderation_status");
            entity.Property(e => e.ReactionCount)
                .HasDefaultValue(0)
                .HasColumnName("reaction_count");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Visibility).HasColumnName("visibility");
        });

        modelBuilder.Entity<PostCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("post_categories_pkey");

            entity.ToTable("post_categories");

            entity.HasIndex(e => e.PostId, "ix_post_categories_post").HasFilter("(deleted_at IS NULL)");

            entity.HasIndex(e => e.CategoryTagId, "ix_post_categories_tag").HasFilter("(deleted_at IS NULL)");

            entity.HasIndex(e => new { e.PostId, e.CategoryTagId }, "ux_post_categories_unique")
                .IsUnique()
                .HasFilter("(deleted_at IS NULL)");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CategoryTagId).HasColumnName("category_tag_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedByAliasId).HasColumnName("created_by_alias_id");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.PostId).HasColumnName("post_id");
        });

        modelBuilder.Entity<PostCounterDelta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("post_counter_deltas_pkey");

            entity.ToTable("post_counter_deltas");

            entity.HasIndex(e => new { e.Processed, e.OccuredAt }, "ix_counter_deltas_unprocessed");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Delta).HasColumnName("delta");
            entity.Property(e => e.Kind).HasColumnName("kind");
            entity.Property(e => e.OccuredAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("occured_at");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.Processed)
                .HasDefaultValue(false)
                .HasColumnName("processed");
        });

        modelBuilder.Entity<PostEmotion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("post_emotions_pkey");

            entity.ToTable("post_emotions");

            entity.HasIndex(e => e.PostId, "ix_post_emotions_post").HasFilter("(deleted_at IS NULL)");

            entity.HasIndex(e => e.EmotionTagId, "ix_post_emotions_tag").HasFilter("(deleted_at IS NULL)");

            entity.HasIndex(e => new { e.PostId, e.EmotionTagId }, "ux_post_emotions_unique")
                .IsUnique()
                .HasFilter("(deleted_at IS NULL)");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedByAliasId).HasColumnName("created_by_alias_id");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.EmotionTagId).HasColumnName("emotion_tag_id");
            entity.Property(e => e.PostId).HasColumnName("post_id");
        });

        modelBuilder.Entity<PostMedium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("post_media_pkey");

            entity.ToTable("post_media");

            entity.HasIndex(e => new { e.PostId, e.Position }, "ix_post_media_post");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.MediaId).HasColumnName("media_id");
            entity.Property(e => e.Position)
                .HasDefaultValue(0)
                .HasColumnName("position");
            entity.Property(e => e.PostId).HasColumnName("post_id");
        });

        modelBuilder.Entity<Reaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reactions_pkey");

            entity.ToTable("reactions");

            entity.HasIndex(e => new { e.TargetType, e.TargetId, e.CreatedAt }, "ix_reactions_target")
                .IsDescending(false, false, true)
                .HasFilter("(deleted_at IS NULL)");

            entity.HasIndex(e => new { e.TargetType, e.TargetId, e.AuthorAliasId, e.ReactionType }, "ux_reactions_unique")
                .IsUnique()
                .HasFilter("(deleted_at IS NULL)");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AuthorAliasId).HasColumnName("author_alias_id");
            entity.Property(e => e.AuthorAliasVersionId).HasColumnName("author_alias_version_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.ReactionType).HasColumnName("reaction_type");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.TargetType).HasColumnName("target_type");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
