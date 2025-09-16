using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Reactions.Queries.GetUserReactionForPost;

public record GetUserReactionForPostQuery(
    Guid PostId,
    Guid AliasId
) : IQuery<GetUserReactionForPostResult>;

public record GetUserReactionForPostResult(
    Guid? ReactionId,
    Guid PostId,
    Guid AliasId,
    string? ReactionCode,
    DateTimeOffset? CreatedAt,
    bool HasReaction
);
