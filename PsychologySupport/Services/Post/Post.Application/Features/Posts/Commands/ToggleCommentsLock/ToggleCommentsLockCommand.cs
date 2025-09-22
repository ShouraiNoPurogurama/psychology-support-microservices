using BuildingBlocks.CQRS;

namespace Post.Application.Features.Posts.Commands.ToggleCommentsLock;

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
