using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Aggregates.Posts.Dtos;

public record PostSummaryDto(
    Guid Id,
    string? Title,
    string Content,
    Guid AuthorAliasId,
    PostVisibility Visibility,
    DateTimeOffset PublishedAt,
    DateTimeOffset? EditedAt,
    int ReactionCount,
    int CommentCount,
    int ViewCount,
    bool HasMedia
);
