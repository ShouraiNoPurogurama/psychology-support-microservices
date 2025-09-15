namespace Post.Application.Aggregates.Posts.Dtos;

public sealed record PostDto(
    Guid Id,
    string Content,
    string? Title,
    AuthorDto Author,
    string Visibility,
    string ModerationStatus,
    int ReactionCount,
    int CommentCount,
    int ViewCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EditedAt,
    DateTimeOffset PublishedAt,
    IReadOnlyList<string> MediaUrls,
    IReadOnlyList<string> Categories
);
