namespace Feed.Domain.PostReplica;

/// <summary>
/// Domain model for the lookup table mapping post IDs to their partition keys.
/// Enables efficient delete and update operations.
/// </summary>
public sealed class PostReplicaById
{
    public Guid PostId { get; }
    public DateOnly YmdBucket { get; }
    public Guid CreatedAt { get; } // TimeUUID

    private PostReplicaById(
        Guid postId,
        DateOnly ymdBucket,
        Guid createdAt)
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
        Guid? createdAt = null)
    {
        var ymdBucketValue = ymdBucket ?? DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        var createdAtValue = createdAt ?? Guid.NewGuid(); // Will be converted to TimeUUID

        return new(
            postId,
            ymdBucketValue,
            createdAtValue);
    }
}
