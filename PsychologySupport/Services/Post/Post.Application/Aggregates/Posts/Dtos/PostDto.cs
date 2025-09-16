using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Aggregates.Posts.Dtos;

public sealed record PostDto(
    Guid Id,
    string Content,
    string? Title,
    AuthorDto Author,
    PostVisibility Visibility,
    ModerationStatus ModerationStatus,
    int ReactionCount,
    int CommentCount,
    int ViewCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EditedAt,
    DateTimeOffset PublishedAt,
    IReadOnlyList<string> MediaUrls,
    IReadOnlyList<string> Categories
);
