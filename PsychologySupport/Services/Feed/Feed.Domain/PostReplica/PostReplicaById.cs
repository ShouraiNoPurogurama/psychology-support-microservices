namespace Feed.Domain.PostReplica;

/// <summary>
/// Domain model for the lookup table mapping post IDs to their partition keys.
/// Enables efficient delete and update operations.
/// </summary>
public sealed class PostReplicaById
{
    public Guid PostId { get; }
    public DateOnly YmdBucket { get; }
    public DateTimeOffset CreatedAt { get; } 

    private PostReplicaById(
        Guid postId,
        DateOnly ymdBucket,
        DateTimeOffset createdAt)
    {
        if (postId == Guid.Empty)
            throw new ArgumentException("PostId is required", nameof(postId));

        PostId = postId;
        YmdBucket = ymdBucket;
        CreatedAt = createdAt;
    }

    public static PostReplicaById Create(
        Guid postId,
        DateOnly? ymdBucket = null,
        DateTimeOffset? createdAt = null)
    {
        var ymdBucketValue = ymdBucket ?? DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        var createdAtValue = createdAt ?? DateTimeOffset.UtcNow;

        return new(
            postId,
            ymdBucketValue,
            createdAtValue);
    }
}
