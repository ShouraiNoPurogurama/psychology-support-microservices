namespace Feed.Domain.FollowerTracking;

public sealed class Follower
{
    public Guid FollowerAliasId { get; }
    public Guid AliasId { get; }
    public DateTimeOffset Since { get; }

    private Follower(Guid FollowerAliasId, Guid aliasId, DateTimeOffset sinceUtc)
    {
        if (FollowerAliasId == Guid.Empty) 
            throw new ArgumentException("FollowerAliasId is required", nameof(FollowerAliasId));
        if (aliasId == Guid.Empty) 
            throw new ArgumentException("AliasId is required", nameof(aliasId));
        if (FollowerAliasId == aliasId)
            throw new ArgumentException("Cannot follow yourself", nameof(aliasId));

        AliasId = aliasId;
        Since = sinceUtc.ToUniversalTime() ;
    }

    public static Follower Create(Guid FollowerAliasId, Guid aliasId, DateTimeOffset? since = null)
        => new(FollowerAliasId, aliasId, (since ?? DateTimeOffset.UtcNow).ToUniversalTime());
}
