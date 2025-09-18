using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Features.Comments.Dtos;

namespace Post.Application.Features.Comments.Queries.GetComments;

public record GetCommentsQuery(
    Guid PostId,
    int Page = 1,
    int PageSize = 20,
    Guid? ParentCommentId = null,
    string SortBy = "CreatedAt",
    bool SortDescending = false
) : IQuery<PaginatedResult<CommentDto>>;

public record HierarchyDto(
    Guid? ParentCommentId,
    string Path,
    int Level
);
