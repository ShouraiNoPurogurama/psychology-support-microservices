using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Reactions.Commands.CreateReaction;

public record CreateReactionCommand(
    string TargetType, // "post" or "comment"
    Guid TargetId,
    string ReactionCode // "like", "heart", "laugh", etc.
) : ICommand<CreateReactionResult>;

public record CreateReactionResult(
    Guid ReactionId,
    string TargetType,
    Guid TargetId,
    string ReactionCode,
    DateTimeOffset CreatedAt
);
