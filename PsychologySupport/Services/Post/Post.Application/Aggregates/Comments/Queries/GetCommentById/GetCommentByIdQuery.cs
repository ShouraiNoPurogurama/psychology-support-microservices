using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Comments.Queries.GetCommentById;

public record GetCommentByIdQuery(
    Guid CommentId
) : IQuery<GetCommentByIdResult>;

public record GetCommentByIdResult(
    CommentDto? Comment
);

public record CommentDto(
    Guid Id,
    Guid PostId,
    Guid AuthorAliasId,
    string AuthorDisplayName,
    string Content,
    int ReplyCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? EditedAt,
    bool IsDeleted
);
