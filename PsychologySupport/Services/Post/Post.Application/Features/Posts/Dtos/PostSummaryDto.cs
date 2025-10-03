using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Dtos;

public record PostSummaryDto(
    Guid Id,
    string? Title,
    string Content,
    bool IsReactedByCurrentUser,
    AuthorDto Author,
    PostVisibility Visibility,
    DateTimeOffset PublishedAt,
    DateTimeOffset? EditedAt,
    int ReactionCount,
    int CommentCount,
    int ViewCount,
    bool HasMedia
);
