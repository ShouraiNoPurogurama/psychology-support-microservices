using Microsoft.EntityFrameworkCore;
using Post.Domain.Aggregates.CategoryTag;
using Post.Domain.Aggregates.Comment;
using Post.Domain.Aggregates.Gift;
using Post.Domain.Aggregates.Post;
using Post.Domain.Aggregates.Reaction;

namespace Post.Application.Data;

/// <summary>
/// Public database context contract for Post service aggregates
/// Exposes DbSets for all domain aggregates and entities following DDD principles
/// </summary>
public interface IPostDbContext
{
    // Post Aggregate
    DbSet<Domain.Aggregates.Post.Post> Posts { get; }
    DbSet<PostMedia> PostMedia { get; }
    DbSet<PostCategory> PostCategories { get; }
    DbSet<PostEmotion> PostEmotions { get; }

    // Gift Aggregate
    DbSet<GiftAttach> GiftAttaches { get; }

    // Comment Aggregate
    DbSet<Comment> Comments { get; }

    // CategoryTag Aggregate
    DbSet<CategoryTag> CategoryTags { get; }

    // Reaction Aggregate
    DbSet<Reaction> Reactions { get; }

    /// <summary>
    /// Save changes asynchronously with domain event processing
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of affected rows</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}