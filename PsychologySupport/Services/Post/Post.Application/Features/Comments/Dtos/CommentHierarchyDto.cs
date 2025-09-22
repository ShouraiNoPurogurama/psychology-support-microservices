namespace Post.Application.Features.Comments.Dtos;

public record CommentHierarchyDto(
    Guid Id,
    string Content,
    Guid AuthorAliasId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EditedAt,
    int ReactionCount,
    IEnumerable<CommentHierarchyDto> Replies
);