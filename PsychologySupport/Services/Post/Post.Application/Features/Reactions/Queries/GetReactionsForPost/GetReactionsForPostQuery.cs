using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Post.Application.Features.Reactions.Dtos;

namespace Post.Application.Features.Reactions.Queries.GetReactionsForPost;

public record GetReactionsForPostQuery(
    Guid PostId,
    int Page = 1,
    int Size = 10
) : IQuery<GetReactionsForPostResult>;

public record GetReactionsForPostResult(
    PaginatedResult<ReactionDto> Reactions
);
