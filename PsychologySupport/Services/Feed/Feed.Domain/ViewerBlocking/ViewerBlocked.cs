namespace Feed.Domain.ViewerBlocking;

public sealed class ViewerBlocked
{
    public Guid AliasId { get; }
    public Guid BlockedAliasId { get; }
    public DateTimeOffset BlockedSince { get; }

    private ViewerBlocked(Guid aliasId, Guid blockedAliasId, DateTimeOffset blockedSinceUtc)
    {
        if (aliasId == Guid.Empty) 
            throw new ArgumentException("Thông tin người dùng không hợp lệ.", nameof(aliasId));
        if (blockedAliasId == Guid.Empty) 
            throw new ArgumentException("Thông tin người dùng không hợp lệ.", nameof(blockedAliasId));
        if (aliasId == blockedAliasId)
            throw new ArgumentException("Không thể chặn chính mình.", nameof(blockedAliasId));

        AliasId = aliasId;
        BlockedAliasId = blockedAliasId;
        BlockedSince = blockedSinceUtc.ToUniversalTime() ;
    }

    public static ViewerBlocked Create(Guid aliasId, Guid blockedAliasId, DateTimeOffset? blockedSince = null)
        => new(aliasId, blockedAliasId, (blockedSince ?? DateTimeOffset.UtcNow).ToUniversalTime());
}
