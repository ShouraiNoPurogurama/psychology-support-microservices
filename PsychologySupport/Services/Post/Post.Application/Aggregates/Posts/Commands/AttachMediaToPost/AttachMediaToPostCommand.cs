using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Posts.Commands.AttachMediaToPost;

public record AttachMediaToPostCommand(
    Guid IdempotencyKey,
    Guid PostId,
    Guid MediaId,
    int? Position = null
) : IdempotentCommand<AttachMediaToPostResult>(IdempotencyKey);

public record AttachMediaToPostResult(
    Guid PostId,
    Guid MediaId,
    DateTimeOffset AttachedAt
);
