namespace Alias.API.Aliases.Dtos;

public record PublicProfileDto(
    Guid AliasId,
    string Label,
    string? AvatarUrl,
    long Followers,
    long Followings,
    long ReactionGivenCount,
    long ReactionReceivedCount,
    int Posts,
    DateTimeOffset CreatedAt);