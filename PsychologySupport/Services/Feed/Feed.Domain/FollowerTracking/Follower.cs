namespace Feed.Domain.FollowerTracking;

public sealed class Follower
{
    public Guid AuthorAliasId { get; }
    public Guid AliasId { get; }
    public DateTimeOffset Since { get; }

    private Follower(Guid authorAliasId, Guid aliasId, DateTimeOffset sinceUtc)
    {
        if (authorAliasId == Guid.Empty) 
            throw new ArgumentException("AuthorAliasId is required", nameof(authorAliasId));
        if (aliasId == Guid.Empty) 
            throw new ArgumentException("AliasId is required", nameof(aliasId));
        if (authorAliasId == aliasId)
            throw new ArgumentException("Cannot follow yourself", nameof(aliasId));

        AuthorAliasId = authorAliasId;
        AliasId = aliasId;
        Since = sinceUtc.ToUniversalTime() ;
    }

    public static Follower Create(Guid authorAliasId, Guid aliasId, DateTimeOffset? since = null)
        => new(authorAliasId, aliasId, (since ?? DateTimeOffset.UtcNow).ToUniversalTime());
}
