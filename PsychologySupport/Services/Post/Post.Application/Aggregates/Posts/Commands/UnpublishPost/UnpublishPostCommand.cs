using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Posts.Commands.UnpublishPost;

public record UnpublishPostCommand(
    Guid IdempotencyKey,
    Guid PostId
) : IdempotentCommand<UnpublishPostResult>(IdempotencyKey);

public record UnpublishPostResult(
    Guid PostId,
    string Status,
    DateTimeOffset UnpublishedAt
);
