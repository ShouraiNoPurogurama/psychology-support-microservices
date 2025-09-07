using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Domain.Models;
using Post.Domain.Models.Public;

namespace Post.Infrastructure.Data.Public;

public partial class PublicDbContext : DbContext, IPublicDbContext
{
    public PublicDbContext(DbContextOptions<PublicDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CategoryTag> CategoryTags { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    // public virtual DbSet<EmotionTag> EmotionTags { get; set; }

    public virtual DbSet<GiftsAttach> GiftsAttaches { get; set; }

    public virtual DbSet<IdempotencyKey> IdempotencyKeys { get; set; }

    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }

    public virtual DbSet<Domain.Models.Public.Post> Posts { get; set; }

    public virtual DbSet<PostCategory> PostCategories { get; set; }

    public virtual DbSet<PostCounterDelta> PostCounterDeltas { get; set; }

    public virtual DbSet<PostEmotion> PostEmotions { get; set; }

    public virtual DbSet<PostMedia> PostMedia { get; set; }

    public virtual DbSet<Reaction> Reactions { get; set; }

    protected override void OnModelCreating(ModelBuilder buidler)
    {
        buidler.HasDefaultSchema("public");
        
        buidler.Entity<CategoryTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("category_tags_pkey");
            entity.ToTable("category_tags");

            entity.HasIndex(e => e.Code, "category_tags_code_key").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
        });

        buidler.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("comments_pkey");
            entity.ToTable("comments");

            entity.HasIndex(e => new { e.PostId, e.CreatedAt, e.Id }, "ix_comments_post_created")
                .IsDescending(false, true, true)
                .HasFilter("((deleted_at IS NULL) AND (moderation_status = 'approved'::text))");

            entity.HasIndex(e => new { e.PostId, e.Path }, "ix_comments_post_path");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.ModerationStatus).HasDefaultValueSql("'pending'::text");
        });

        // modelBuilder.Entity<EmotionTag>(entity =>
        // {
        //     entity.HasKey(e => e.Id).HasName("emotion_tags_pkey");
        //     entity.ToTable("emotion_tags");
        //
        //     entity.HasIndex(e => e.Code, "emotion_tags_code_key").IsUnique();
        //
        //     entity.Property(e => e.Id).ValueGeneratedNever();
        //     entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        //     entity.Property(e => e.IsActive).HasDefaultValue(true);
        //     entity.Property(e => e.SortOrder).HasDefaultValue(0);
        // });

        buidler.Entity<GiftsAttach>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("gifts_attach_pkey");
            entity.ToTable("gifts_attach");

            entity.HasIndex(e => new { e.TargetType, e.TargetId, e.CreatedAt }, "ix_gifts_target")
                .IsDescending(false, false, true)
                .HasFilter("(deleted_at IS NULL)");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        });

        buidler.Entity<IdempotencyKey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("idempotency_keys_pkey");
            entity.ToTable("idempotency_keys");

            entity.HasIndex(e => e.Key, "idempotency_keys_idempotency_key").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        });

        buidler.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("outbox_messages_pkey");
            entity.ToTable("outbox_messages");

            entity.HasIndex(e => e.OccuredOn, "ix_outbox_pending")
                .HasFilter("(processed_on IS NULL)");
            entity.HasIndex(e => e.ProcessedOn, "ix_outbox_processed")
                .HasFilter("(processed_on IS NOT NULL)");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        buidler.Entity<Domain.Models.Public.Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("posts_pkey");
            entity.ToTable("posts");

            entity.HasIndex(e => new { e.AuthorAliasId, e.CreatedAt }, "ix_posts_author")
                .IsDescending(false, true)
                .HasFilter("(deleted_at IS NULL)");

            entity.HasIndex(e => new { e.CreatedAt, e.Id }, "ix_posts_list")
                .IsDescending()
                .HasFilter("((deleted_at IS NULL) AND (moderation_status = 'approved'::text))");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CommentCount).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.ModerationReasons).HasDefaultValueSql("'{}'::text[]");
            entity.Property(e => e.ModerationStatus).HasDefaultValueSql("'pending'::text");
            entity.Property(e => e.ReactionCount).HasDefaultValue(0);
        });

        buidler.Entity<PostCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("post_categories_pkey");
            entity.ToTable("post_categories");

            entity.HasIndex(e => e.PostId, "ix_post_categories_post")
                .HasFilter("(deleted_at IS NULL)");
            entity.HasIndex(e => e.CategoryTagId, "ix_post_categories_tag")
                .HasFilter("(deleted_at IS NULL)");
            entity.HasIndex(e => new { e.PostId, e.CategoryTagId }, "ux_post_categories_unique")
                .IsUnique()
                .HasFilter("(deleted_at IS NULL)");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        });

        buidler.Entity<PostCounterDelta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("post_counter_deltas_pkey");
            entity.ToTable("post_counter_deltas");

            entity.HasIndex(e => new { e.Processed, e.OccuredAt }, "ix_counter_deltas_unprocessed");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.OccuredAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Processed).HasDefaultValue(false);
        });

        buidler.Entity<PostEmotion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("post_emotions_pkey");
            entity.ToTable("post_emotions");

            entity.HasIndex(e => e.PostId, "ix_post_emotions_post")
                .HasFilter("(deleted_at IS NULL)");
            entity.HasIndex(e => e.EmotionTagId, "ix_post_emotions_tag")
                .HasFilter("(deleted_at IS NULL)");
            entity.HasIndex(e => new { e.PostId, e.EmotionTagId }, "ux_post_emotions_unique")
                .IsUnique()
                .HasFilter("(deleted_at IS NULL)");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        });

        buidler.Entity<PostMedia>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("post_media_pkey");
            entity.ToTable("post_media");

            entity.HasIndex(e => new { e.PostId, e.Position }, "ix_post_media_post");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.Position).HasDefaultValue(0);
        });

        buidler.Entity<Reaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reactions_pkey");
            entity.ToTable("reactions");

            entity.HasIndex(e => new { e.TargetType, e.TargetId, e.CreatedAt }, "ix_reactions_target")
                .IsDescending(false, false, true)
                .HasFilter("(deleted_at IS NULL)");

            entity.HasIndex(e => new { e.TargetType, e.TargetId, e.AuthorAliasId, e.ReactionType }, "ux_reactions_unique")
                .IsUnique()
                .HasFilter("(deleted_at IS NULL)");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        });

        OnModelCreatingPartial(buidler);
    }


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}