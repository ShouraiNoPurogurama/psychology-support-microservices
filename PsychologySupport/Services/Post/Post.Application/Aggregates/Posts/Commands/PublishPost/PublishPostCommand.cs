using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Posts.Commands.PublishPost;

public record PublishPostCommand(
    Guid IdempotencyKey,
    Guid PostId
) : IdempotentCommand<PublishPostResult>(IdempotencyKey);

public record PublishPostResult(
    Guid PostId,
    string Status,
    DateTimeOffset PublishedAt
);
