using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Posts.Commands.AttachMediaToPost;

public record AttachMediaToPostCommand(
    Guid IdempotencyKey,
    Guid PostId,
    IEnumerable<Guid> MediaIds
) : IdempotentCommand<AttachMediaToPostResult>(IdempotencyKey);

public record AttachMediaToPostResult(
    Guid PostId,
    IEnumerable<Guid> AttachedMediaIds,
    DateTimeOffset AttachedAt
);
