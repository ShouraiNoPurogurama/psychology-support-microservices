namespace Post.Application.Aggregates.Comments.Dtos;

public record CommentHierarchyDto(
    Guid Id,
    string Content,
    Guid AuthorAliasId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EditedAt,
    int ReactionCount,
    IEnumerable<CommentHierarchyDto> Replies
);