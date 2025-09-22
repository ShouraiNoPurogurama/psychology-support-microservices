using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Features.Posts.Dtos;

namespace Post.Application.Features.Posts.Queries.GetPostsByAliasIds;

public record GetPostsByAliasIdsQuery(
    IEnumerable<Guid> AliasIds,
    int Page = 1,
    int Size = 10
) : IQuery<GetPostsByAliasIdsResult>;

public record GetPostsByAliasIdsResult(
    PaginatedResult<PostSummaryDto> Posts
);