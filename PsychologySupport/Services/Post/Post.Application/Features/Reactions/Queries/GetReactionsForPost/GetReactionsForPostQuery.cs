using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;

namespace Post.Application.Features.Reactions.Queries.GetReactionsForPost;

public record GetReactionsForPostQuery(
    Guid PostId,
    int Page = 1,
    int Size = 10
) : IQuery<GetReactionsForPostResult>;

public record GetReactionsForPostResult(
    PaginatedResult<ReactionDto> Reactions
);

public record ReactionDto(
    Guid Id,
    Guid PostId,
    Guid AliasId,
    string AliasDisplayName,
    string ReactionCode,
    DateTimeOffset CreatedAt
);
