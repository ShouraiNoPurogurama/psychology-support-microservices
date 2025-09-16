using Post.Domain.Aggregates.CategoryTags;

namespace Post.Domain.Aggregates.Posts;

public sealed class PostCategory : Entity<Guid>, ISoftDeletable
{
    public Guid PostId { get; set; }
    public Guid CategoryTagId { get; set; }
    public DateTimeOffset AssignedAt { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedByAliasId { get; set; }

    public CategoryTag CategoryTag { get; set; }
    
    private PostCategory()
    {
    }

    public static PostCategory Create(Guid postId, Guid categoryTagId)
    {
        return new PostCategory
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            CategoryTagId = categoryTagId,
            AssignedAt = DateTimeOffset.UtcNow
        };
    }
}