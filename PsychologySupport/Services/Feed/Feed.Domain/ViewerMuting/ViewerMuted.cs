namespace Feed.Domain.ViewerMuting;

public sealed class ViewerMuted
{
    public Guid AliasId { get; }
    public Guid MutedAliasId { get; }
    public DateTimeOffset MutedSince { get; }

    private ViewerMuted(Guid aliasId, Guid mutedAliasId, DateTimeOffset mutedSinceUtc)
    {
        if (aliasId == Guid.Empty) 
            throw new ArgumentException("AliasId is required", nameof(aliasId));
        if (mutedAliasId == Guid.Empty) 
            throw new ArgumentException("MutedAliasId is required", nameof(mutedAliasId));
        if (aliasId == mutedAliasId)
            throw new ArgumentException("Cannot mute yourself", nameof(mutedAliasId));

        AliasId = aliasId;
        MutedAliasId = mutedAliasId;
        MutedSince =mutedSinceUtc.ToUniversalTime();
    }

    public static ViewerMuted Create(Guid aliasId, Guid mutedAliasId, DateTimeOffset? mutedSince = null)
        => new(aliasId, mutedAliasId, (mutedSince ?? DateTimeOffset.UtcNow).ToUniversalTime());
}
