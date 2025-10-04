using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Features.Comments.Dtos;

namespace Post.Application.Features.Comments.Queries.GetCommentReplies;

public record GetCommentRepliesQuery(
    Guid ParentCommentId,
    int PageIndex = 1,
    int PageSize = 10
) : IQuery<GetCommentRepliesResult>;

public record GetCommentRepliesResult(
    PaginatedResult<ReplySummaryDto> Replies
);
