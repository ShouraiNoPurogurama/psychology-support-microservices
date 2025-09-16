using BuildingBlocks.CQRS;
using Post.Domain.Aggregates.Reactions.Enums;

namespace Post.Application.Aggregates.Reactions.Commands.RemoveReaction;

public record RemoveReactionCommand(
    ReactionTargetType TargetType,
    Guid TargetId
) : ICommand<RemoveReactionResult>;

public record RemoveReactionResult(
    bool WasRemoved,
    DateTimeOffset RemovedAt
);
