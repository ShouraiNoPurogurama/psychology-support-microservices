namespace Post.Application.Features.Posts.Dtos;

public sealed record AuthorDto(
    Guid AliasId,
    string DisplayName,
    string? AvatarUrl //todo gắn default url sau
);
