namespace Feed.Domain.PostReplica;

/// <summary>
/// Domain model for the pre-filtered query table containing only public, finalized posts.
/// Optimized for fast retrieval without ALLOW FILTERING.
/// </summary>
public sealed class PostPublicFinalizedByDay
{
    public DateOnly YmdBucket { get; }
    public DateTimeOffset CreatedAt { get; } // TimeUUID
    public Guid PostId { get; }
    public Guid AuthorAliasId { get; }

    private PostPublicFinalizedByDay(
        DateOnly ymdBucket,
        DateTimeOffset createdAt,
        Guid postId,
        Guid authorAliasId)
    {
        if (postId == Guid.Empty)
            throw new ArgumentException("PostId is required", nameof(postId));
        if (authorAliasId == Guid.Empty)
            throw new ArgumentException("AuthorAliasId is required", nameof(authorAliasId));

        YmdBucket = ymdBucket;
        CreatedAt = createdAt;
        PostId = postId;
        AuthorAliasId = authorAliasId;
    }

    public static PostPublicFinalizedByDay Create(
        Guid postId,
        Guid authorAliasId,
        DateOnly? ymdBucket = null,
        DateTimeOffset? createdAt = null)
    {
        var ymdBucketValue = ymdBucket ?? DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        var createdAtValue = createdAt ??DateTimeOffset.UtcNow; 

        return new(
            ymdBucketValue,
            createdAtValue,
            postId,
            authorAliasId);
    }
}
