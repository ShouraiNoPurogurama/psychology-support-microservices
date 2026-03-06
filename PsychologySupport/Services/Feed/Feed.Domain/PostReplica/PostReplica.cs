namespace Feed.Domain.PostReplica;

/// <summary>
/// Domain model representing a replicated post entry.
/// Used for general post data replication with day-based partitioning.
/// </summary>
public sealed class PostReplica
{
    public DateOnly YmdBucket { get; }
    public DateTimeOffset CreatedAt { get; } // TimeUUID
    public Guid PostId { get; }
    public Guid AuthorAliasId { get; }
    public string Visibility { get; }
    public string Status { get; }

    private PostReplica(
        DateOnly ymdBucket,
        DateTimeOffset createdAt,
        Guid postId,
        Guid authorAliasId,
        string visibility,
        string status)
    {
        if (postId == Guid.Empty)
            throw new ArgumentException("Thông tin bài viết không hợp lệ.", nameof(postId));
        if (authorAliasId == Guid.Empty)
            throw new ArgumentException("Thông tin tác giả không hợp lệ.", nameof(authorAliasId));
        if (string.IsNullOrWhiteSpace(visibility))
            throw new ArgumentException("Thông tin hiển thị không hợp lệ.", nameof(visibility));
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Trạng thái không hợp lệ.", nameof(status));

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
        DateTimeOffset? createdAt = null)
    {
        var ymdBucketValue = ymdBucket ?? DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        var createdAtValue = createdAt ?? DateTimeOffset.UtcNow; 

        return new(
            ymdBucketValue,
            createdAtValue,
            postId,
            authorAliasId,
            visibility,
            status);
    }
}
