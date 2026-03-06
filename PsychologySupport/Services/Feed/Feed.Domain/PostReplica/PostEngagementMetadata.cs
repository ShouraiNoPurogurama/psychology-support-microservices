namespace Feed.Domain.PostReplica;

/// <summary>
/// Domain model representing post engagement metadata.
/// Stores author information and timestamp tracking for engagement updates.
/// </summary>
public sealed class PostEngagementMetadata
{
    public Guid PostId { get; }
    public Guid AuthorAliasId { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset CountersLastUpdated { get; }

    private PostEngagementMetadata(
        Guid postId,
        Guid authorAliasId,
        DateTimeOffset createdAt,
        DateTimeOffset countersLastUpdated)
    {
        if (postId == Guid.Empty)
            throw new ArgumentException("Thông tin bài viết không hợp lệ.", nameof(postId));
        if (authorAliasId == Guid.Empty)
            throw new ArgumentException("Thông tin tác giả không hợp lệ.", nameof(authorAliasId));

        PostId = postId;
        AuthorAliasId = authorAliasId;
        CreatedAt = createdAt;
        CountersLastUpdated = countersLastUpdated;
    }

    public static PostEngagementMetadata Create(
        Guid postId,
        Guid authorAliasId,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? countersLastUpdated = null)
    {
        var createdAtValue = createdAt ?? DateTimeOffset.UtcNow;
        var countersLastUpdatedValue = countersLastUpdated ?? DateTimeOffset.UtcNow;

        return new(
            postId,
            authorAliasId,
            createdAtValue,
            countersLastUpdatedValue);
    }

    public PostEngagementMetadata WithUpdatedCountersTimestamp(DateTimeOffset? timestamp = null)
    {
        var updatedTimestamp = timestamp ?? DateTimeOffset.UtcNow;
        return new(PostId, AuthorAliasId, CreatedAt, updatedTimestamp);
    }
}
