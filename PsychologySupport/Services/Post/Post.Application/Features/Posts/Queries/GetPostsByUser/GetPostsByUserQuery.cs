using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Features.Posts.Dtos;

namespace Post.Application.Features.Posts.Queries.GetPostsByUser;

public record GetPostsByUserQuery(
    Guid AuthorAliasId,
    int PageNumber,
    int PageSize
) : IQuery<GetPostsByUserResult>;

public record GetPostsByUserResult(
    PaginatedResult<PostDto> Posts
);
