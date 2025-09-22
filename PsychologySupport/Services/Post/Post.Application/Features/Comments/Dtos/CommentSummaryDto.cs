namespace Post.Application.Features.Comments.Dtos;

public record CommentSummaryDto(
    Guid Id,
    string Content,
    Guid AuthorAliasId,
    DateTimeOffset CreatedAt,
    int ReactionCount,
    bool HasReplies
);