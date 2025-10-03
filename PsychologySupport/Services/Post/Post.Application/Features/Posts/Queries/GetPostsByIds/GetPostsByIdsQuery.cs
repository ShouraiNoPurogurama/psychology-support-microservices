using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Features.Posts.Dtos;

namespace Post.Application.Features.Posts.Queries.GetPostsByIds;

public record GetPostsByIdsQuery(
    int PageIndex = 1,
    int PageSize = 20,
    
    string? SortBy = "CreatedAt",
    bool SortDescending = true
) : IQuery<PaginatedResult<PostSummaryDto>>;

