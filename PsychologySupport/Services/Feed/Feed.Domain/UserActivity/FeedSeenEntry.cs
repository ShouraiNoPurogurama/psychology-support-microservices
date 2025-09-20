namespace Feed.Domain.UserActivity;

public sealed class FeedSeenEntry
{
    public Guid AliasId { get; }
    public DateOnly Ymd { get; }
    public Guid SeenAt { get; }
    public Guid PostId { get; }

    private FeedSeenEntry(Guid aliasId, DateOnly ymd, Guid seenAt, Guid postId)
    {
        if (aliasId == Guid.Empty)
            throw new ArgumentException("AliasId is required", nameof(aliasId));
        if (postId == Guid.Empty)
            throw new ArgumentException("PostId is required", nameof(postId));

        AliasId = aliasId;
        Ymd = ymd;
        SeenAt = seenAt;
        PostId = postId;
    }

    public static FeedSeenEntry Create(Guid aliasId, Guid postId, DateOnly? ymd = null, Guid? seenAt = null)
    {
        var seenAtValue = seenAt ?? Guid.NewGuid();
        var ymdValue = ymd ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
        
        return new(aliasId, ymdValue, seenAtValue, postId);
    }
}
