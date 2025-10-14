namespace Alias.API.Aliases.Models.Aliases.ValueObjects;

public sealed record AliasMetadata
{
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastActiveAt { get; init; }
    public int VersionCount { get; init; }
    public bool IsSystemGenerated { get; init; }

    //Properties for social media tracking
    public long FollowersCount { get; private init; }
    public long FollowingCount { get; private init; }
    
    public long ReactionGivenCount { get; private init; }
    public long ReactionReceivedCount { get; private init; }
    //For analytics purpose
    public long CommentsCount { get; private init; }
    public long SharesCount { get; private init; }

    // EF Core materialization
    private AliasMetadata()
    {
    }

    private AliasMetadata(DateTimeOffset createdAt, DateTimeOffset? lastActiveAt, int versionCount, bool isSystemGenerated)
    {
        CreatedAt = createdAt;
        LastActiveAt = lastActiveAt;
        VersionCount = versionCount;
        IsSystemGenerated = isSystemGenerated;
    }

    public static AliasMetadata Create(bool isSystemGenerated = false)
    {
        return new AliasMetadata(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            1,
            isSystemGenerated);
    }

    public AliasMetadata IncrementVersionCount()
    {
        return this with { VersionCount = VersionCount + 1 };
    }

    public AliasMetadata UpdateLastActive()
    {
        return this with { LastActiveAt = DateTimeOffset.UtcNow };
    }

    public AliasMetadata IncrementFollowersCount()
    {
        return this with { FollowersCount = FollowersCount + 1 };
    }

    public AliasMetadata DecrementFollowersCount()
    {
        // Đảm bảo số đếm không bao giờ âm
        return this with { FollowersCount = FollowersCount > 0 ? FollowersCount - 1 : 0 };
    }

    public AliasMetadata IncrementFollowingCount()
    {
        return this with { FollowingCount = FollowingCount + 1 };
    }

    public AliasMetadata DecrementFollowingCount()
    {
        return this with { FollowingCount = FollowingCount > 0 ? FollowingCount - 1 : 0 };
    }
    
    public AliasMetadata IncrementReactionGivenCount()
    {
        return this with { ReactionGivenCount = ReactionGivenCount + 1 };
    }
    
    public AliasMetadata IncrementReactionReceivedCount()
    {
        return this with { ReactionReceivedCount = ReactionReceivedCount + 1 };
    }  
    
    public AliasMetadata IncrementCommentsCount()
    {
        return this with { CommentsCount = CommentsCount + 1 };
    }
    
    public AliasMetadata IncrementSharesCount()
    {
        return this with { SharesCount = SharesCount + 1 };
    }
}