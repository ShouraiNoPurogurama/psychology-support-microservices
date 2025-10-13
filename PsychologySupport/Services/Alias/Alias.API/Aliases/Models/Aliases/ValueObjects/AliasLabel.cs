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