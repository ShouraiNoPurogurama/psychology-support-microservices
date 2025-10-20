using BuildingBlocks.CQRS;
using Post.Domain.Aggregates.Reaction.Enums;

namespace Post.Application.Features.Reactions.Commands.CreateReaction;

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
