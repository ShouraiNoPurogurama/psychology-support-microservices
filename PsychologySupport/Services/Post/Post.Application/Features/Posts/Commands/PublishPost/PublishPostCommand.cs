using BuildingBlocks.CQRS;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Commands.PublishPost;

public record PublishPostCommand(
    Guid IdempotencyKey,
    Guid PostId
) : IdempotentCommand<PublishPostResult>(IdempotencyKey);

public record PublishPostResult(
    Guid PostId,
    PostVisibility Visibility,
    DateTimeOffset PublishedAt
);
