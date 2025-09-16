using BuildingBlocks.CQRS;
using Post.Domain.Aggregates.Reactions.Enums;

namespace Post.Application.Aggregates.Reactions.Commands.CreateReaction;

public record CreateReactionCommand(
    ReactionTargetType TargetType,
    Guid TargetId,
    ReactionCode ReactionCode
) : ICommand<CreateReactionResult>;

public record CreateReactionResult(
    Guid ReactionId,
    ReactionTargetType TargetType,
    Guid TargetId,
    ReactionCode ReactionCode,
    DateTimeOffset CreatedAt
);
