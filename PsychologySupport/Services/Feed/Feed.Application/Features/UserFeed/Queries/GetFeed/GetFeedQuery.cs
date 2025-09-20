using BuildingBlocks.CQRS;

namespace Feed.Application.Features.UserFeed.Queries.GetFeed;

public record GetFeedQuery(
    Guid AliasId,
    int PageIndex = 0,
    int PageSize = 20,
    string? Cursor = null
) : IQuery<GetFeedResult>;
