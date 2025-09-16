using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Reactions.Queries.GetReactionSummaryForPost;

public record GetReactionSummaryForPostQuery(
    Guid PostId
) : IQuery<GetReactionSummaryForPostResult>;

public record GetReactionSummaryForPostResult(
    Guid PostId,
    int TotalReactions,
    Dictionary<string, int> ReactionCounts
);
