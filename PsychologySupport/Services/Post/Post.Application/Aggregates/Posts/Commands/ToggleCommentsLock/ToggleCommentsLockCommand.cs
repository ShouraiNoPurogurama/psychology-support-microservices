using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Posts.Commands.ToggleCommentsLock;

public record ToggleCommentsLockCommand(
    Guid IdempotencyKey,
    Guid PostId,
    bool IsLocked
) : IdempotentCommand<ToggleCommentsLockResult>(IdempotencyKey);

public record ToggleCommentsLockResult(
    Guid PostId,
    bool IsCommentsLocked,
    DateTimeOffset UpdatedAt
);
