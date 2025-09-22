namespace Feed.Domain.ViewerFollowing;

public sealed class ViewerFollow
{
    public Guid AliasId { get; }
    public Guid FollowedAliasId { get; }
    public DateTimeOffset Since { get; }

    private ViewerFollow(Guid aliasId, Guid followedAliasId, DateTimeOffset sinceUtc)
    {
        if (aliasId == Guid.Empty) 
            throw new ArgumentException("AliasId is required", nameof(aliasId));
        if (followedAliasId == Guid.Empty) 
            throw new ArgumentException("FollowedAliasId is required", nameof(followedAliasId));
        if (aliasId == followedAliasId)
            throw new ArgumentException("Cannot follow yourself", nameof(followedAliasId));

        AliasId = aliasId;
        FollowedAliasId = followedAliasId;
        Since = sinceUtc.ToUniversalTime();
    }

    public static ViewerFollow Create(Guid aliasId, Guid followedAliasId, DateTimeOffset? since = null)
        => new(aliasId, followedAliasId, (since ?? DateTimeOffset.UtcNow).ToUniversalTime());
}
