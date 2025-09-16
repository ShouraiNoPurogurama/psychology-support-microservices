namespace Post.Application.Aggregates.Comments.Dtos;

public record CommentDto(
    Guid Id,
    Guid PostId,
    string Content,
    Guid AuthorAliasId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EditedAt,
    int ReactionCount,
    int ReplyCount,
    bool IsDeleted,
    Guid? ParentCommentId
);