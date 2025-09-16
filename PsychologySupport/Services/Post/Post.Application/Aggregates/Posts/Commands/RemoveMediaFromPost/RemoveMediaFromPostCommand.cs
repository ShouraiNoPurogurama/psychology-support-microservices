using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Posts.Commands.RemoveMediaFromPost;

public record RemoveMediaFromPostCommand(
    Guid IdempotencyKey,
    Guid PostId,
    Guid MediaId
) : IdempotentCommand<RemoveMediaFromPostResult>(IdempotencyKey);

public record RemoveMediaFromPostResult(
    Guid PostId,
    Guid RemovedMediaId,
    DateTimeOffset RemovedAt
);
