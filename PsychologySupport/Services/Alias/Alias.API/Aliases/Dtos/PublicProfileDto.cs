namespace Alias.API.Aliases.Dtos;

public record PublicProfileDto(
    Guid AliasId,
    string Label,
    string? AvatarUrl,
    int Followers,
    int Followings,
    int Posts,
    DateTimeOffset CreatedAt);