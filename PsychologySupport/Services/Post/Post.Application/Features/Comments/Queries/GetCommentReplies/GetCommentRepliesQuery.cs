using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Features.Comments.Queries.GetCommentsByPost;

namespace Post.Application.Features.Comments.Queries.GetCommentReplies;

public record GetCommentRepliesQuery(
    Guid ParentCommentId,
    int Page = 1,
    int Size = 10
) : IQuery<GetCommentRepliesResult>;

public record GetCommentRepliesResult(
    PaginatedResult<CommentReplyDto> Replies
);
