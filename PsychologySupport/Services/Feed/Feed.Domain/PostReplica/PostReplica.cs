namespace Feed.Domain.PostReplica;

/// <summary>
/// Domain model representing a replicated post entry.
/// Used for general post data replication with day-based partitioning.
/// </summary>
public sealed class PostReplica
{
    public DateOnly YmdBucket { get; }
    public Guid CreatedAt { get; } // TimeUUID
    public Guid PostId { get; }
    public Guid AuthorAliasId { get; }
    public string Visibility { get; }
    public string Status { get; }

    private PostReplica(
        DateOnly ymdBucket,
        Guid createdAt,
        Guid postId,
        Guid authorAliasId,
        string visibility,
        string status)
    {
        if (postId == Guid.Empty)
            throw new ArgumentException("PostId is required", nameof(postId));
        if (authorAliasId == Guid.Empty)
            throw new ArgumentException("AuthorAliasId is required", nameof(authorAliasId));
        if (string.IsNullOrWhiteSpace(visibility))
            throw new ArgumentException("Visibility is required", nameof(visibility));
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status is required", nameof(status));

        YmdBucket = ymdBucket;
        CreatedAt = createdAt;
        PostId = postId;
        AuthorAliasId = authorAliasId;
        Visibility = visibility;
        Status = status;
    }

    public static PostReplica Create(
        Guid postId,
        Guid authorAliasId,
        string visibility,
        string status,
        DateOnly? ymdBucket = null,
        Guid? createdAt = null)
    {
        var ymdBucketValue = ymdBucket ?? DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        var createdAtValue = createdAt ?? Guid.NewGuid(); // Will be converted to TimeUUID

        return new(
            ymdBucketValue,
            createdAtValue,
            postId,
            authorAliasId,
            visibility,
            status);
    }
}
