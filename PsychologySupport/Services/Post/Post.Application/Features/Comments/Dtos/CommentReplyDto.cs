namespace Post.Application.Features.Comments.Dtos;

public record CommentReplyDto(
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
