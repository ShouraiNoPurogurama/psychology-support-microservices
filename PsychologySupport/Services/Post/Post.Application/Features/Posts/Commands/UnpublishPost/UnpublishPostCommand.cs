using BuildingBlocks.CQRS;

namespace Post.Application.Features.Posts.Commands.UnpublishPost;

public record UnpublishPostCommand(
    Guid IdempotencyKey,
    Guid PostId
) : IdempotentCommand<UnpublishPostResult>(IdempotencyKey);

public record UnpublishPostResult(
    Guid PostId,
    string Status,
    DateTimeOffset UnpublishedAt
);
