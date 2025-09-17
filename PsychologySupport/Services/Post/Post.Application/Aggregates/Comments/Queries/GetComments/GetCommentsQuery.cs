using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Aggregates.Comments.Dtos;
using Post.Application.Aggregates.Posts.Dtos;

namespace Post.Application.Aggregates.Comments.Queries.GetComments;

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
