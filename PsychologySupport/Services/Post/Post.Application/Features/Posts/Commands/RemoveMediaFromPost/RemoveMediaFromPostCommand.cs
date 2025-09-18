using BuildingBlocks.CQRS;

namespace Post.Application.Features.Posts.Commands.RemoveMediaFromPost;

public record RemoveMediaFromPostCommand(
    Guid IdempotencyKey,
    Guid PostId,
    Guid MediaId
) : IdempotentCommand<RemoveMediaFromPostResult>(IdempotencyKey);

public record RemoveMediaFromPostResult(
    Guid PostId,
    Guid MediaId,
    DateTimeOffset RemovedAt
);
