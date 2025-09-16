using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Posts.Commands.SetPostCoverMedia;

public record SetPostCoverMediaCommand(
    Guid IdempotencyKey,
    Guid PostId,
    Guid MediaId
) : IdempotentCommand<SetPostCoverMediaResult>(IdempotencyKey);

public record SetPostCoverMediaResult(
    Guid PostId,
    Guid CoverMediaId,
    DateTimeOffset UpdatedAt
);
