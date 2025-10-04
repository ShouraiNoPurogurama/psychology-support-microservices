using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Features.Comments.Dtos;

namespace Post.Application.Features.Comments.Queries.GetComments;

public record GetCommentsQuery(
    Guid PostId,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "CreatedAt",
    bool SortDescending = false
) : IQuery<GetCommentsResult>;

public record GetCommentsResult(
    PaginatedResult<CommentSummaryDto> Comments
);
