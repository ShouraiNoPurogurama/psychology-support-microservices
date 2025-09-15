namespace Post.Domain.Aggregates.Post;

public sealed class PostCategory : Entity<Guid>, ISoftDeletable
{
    public Guid PostId { get; set; }
    public Guid CategoryTagId { get; set; }
    public DateTime AssignedAt { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedByAliasId { get; set; }

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
            AssignedAt = DateTime.UtcNow
        };
    }
}