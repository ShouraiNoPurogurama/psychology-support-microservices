using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Reactions.Commands.RemoveReaction;

public record RemoveReactionCommand(
    string TargetType,
    Guid TargetId
) : ICommand<RemoveReactionResult>;

public record RemoveReactionResult(
    bool WasRemoved,
    DateTimeOffset RemovedAt
);
