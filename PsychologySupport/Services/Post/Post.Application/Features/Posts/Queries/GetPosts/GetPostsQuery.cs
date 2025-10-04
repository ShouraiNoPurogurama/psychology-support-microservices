using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.AspNetCore.Mvc;
using Post.Application.Features.Posts.Dtos;

namespace Post.Application.Features.Posts.Queries.GetPosts;

public record GetPostsQuery(
    List<Guid> Ids,
    int PageIndex = 1,
    int PageSize = 20,
    string? Visibility = null,
    List<Guid>? CategoryTagIds = null,
    string? SortBy = "CreatedAt",
    bool SortDescending = true
) : IQuery<GetPostsResult>;

public record GetPostsResult(
    PaginatedResult<PostSummaryDto> Posts
);
