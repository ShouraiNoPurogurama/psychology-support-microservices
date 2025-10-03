using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Features.Comments.Dtos;

namespace Post.Application.Features.Comments.Queries.GetCommentsByPost;

public record GetCommentsByPostQuery(
    Guid PostId,
    int Page = 1,
    int Size = 10
) : IQuery<GetCommentsByPostResult>;

public record GetCommentsByPostResult(
    PaginatedResult<CommentReplyDto> Comments
);
