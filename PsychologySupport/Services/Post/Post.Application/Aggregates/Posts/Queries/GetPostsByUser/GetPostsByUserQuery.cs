using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Aggregates.Posts.Dtos;

namespace Post.Application.Aggregates.Posts.Queries.GetPostsByUser;

public record GetPostsByUserQuery(
    Guid AuthorAliasId,
    int PageNumber,
    int PageSize
) : IQuery<PaginatedResult<PostDto>>;
