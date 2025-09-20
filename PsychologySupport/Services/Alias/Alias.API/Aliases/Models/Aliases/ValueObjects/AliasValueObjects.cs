using Alias.API.Aliases.Exceptions.DomainExceptions;

namespace Alias.API.Aliases.Models.Aliases.ValueObjects;

public sealed record AliasLabel
{
    public string Value { get; init; }
    public string SearchKey { get; init; }
    public string UniqueKey { get; init; }

    // EF Core materialization
    private AliasLabel()
    {
    }

    private AliasLabel(string value, string searchKey, string uniqueKey)
    {
        Value = value;
        SearchKey = searchKey;
        UniqueKey = uniqueKey;
    }

    public static AliasLabel Create(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new InvalidAliasDataException("Alias label cannot be empty.");

        if (label.Length < 2)
            throw new InvalidAliasDataException("Alias label must be at least 2 characters long.");

        if (label.Length > 50)
            throw new InvalidAliasDataException("Alias label cannot exceed 50 characters.");

        var normalizedLabel = label.Trim();
        var searchKey = GenerateSearchKey(normalizedLabel);
        var uniqueKey = GenerateUniqueKey(normalizedLabel);

        return new AliasLabel(normalizedLabel, searchKey, uniqueKey);
    }

    private static string GenerateSearchKey(string label)
    {
        // Remove diacritics and normalize for search
        return label.ToUpperInvariant()
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("_", "");
    }

    private static string GenerateUniqueKey(string label)
    {
        // Generate a unique key for collision detection
        return label.ToLowerInvariant()
            .Replace(" ", "_")
            .Replace("-", "_");
    }
}

public sealed record AliasMetadata
{
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastActiveAt { get; init; }
    public int VersionCount { get; init; }
    public bool IsSystemGenerated { get; init; }

    //Properties for social media tracking
    public long FollowersCount { get; private init; }
    public long FollowingCount { get; private init; }

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
}