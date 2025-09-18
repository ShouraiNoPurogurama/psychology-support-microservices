using BuildingBlocks.CQRS;

namespace Post.Application.Features.Posts.Commands.RestorePost;

public record RestorePostCommand(
    Guid IdempotencyKey,
    Guid PostId
) : IdempotentCommand<RestorePostResult>(IdempotencyKey);

public record RestorePostResult(
    Guid PostId,
    string Status,
    DateTimeOffset RestoredAt
);
