namespace Feed.Domain.FollowerTracking;

public sealed class Follower
{
    public Guid FollowerAliasId { get; }
    public Guid AliasId { get; }
    public DateTimeOffset Since { get; }

    private Follower(Guid FollowerAliasId, Guid aliasId, DateTimeOffset sinceUtc)
    {
        if (FollowerAliasId == Guid.Empty) 
            throw new ArgumentException("Thông tin người dùng không hợp lệ.", nameof(FollowerAliasId));
        if (aliasId == Guid.Empty) 
            throw new ArgumentException("Thông tin người dùng không hợp lệ.", nameof(aliasId));
        if (FollowerAliasId == aliasId)
            throw new ArgumentException("Không thể theo dõi chính mình.", nameof(aliasId));

        AliasId = aliasId;
        Since = sinceUtc.ToUniversalTime() ;
    }

    public static Follower Create(Guid FollowerAliasId, Guid aliasId, DateTimeOffset? since = null)
        => new(FollowerAliasId, aliasId, (since ?? DateTimeOffset.UtcNow).ToUniversalTime());
}
