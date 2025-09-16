using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;

namespace Post.Application.Aggregates.Comments.Queries.GetCommentsByPost;

public record GetCommentsByPostQuery(
    Guid PostId,
    int Page = 1,
    int Size = 10
) : IQuery<GetCommentsByPostResult>;

public record GetCommentsByPostResult(
    PaginatedResult<CommentReplyDto> Comments
);

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
