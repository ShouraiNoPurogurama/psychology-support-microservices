using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Posts.Commands.ReorderPostMedia;

public record ReorderPostMediaCommand(
    Guid IdempotencyKey,
    Guid PostId,
    List<Guid> OrderedMediaIds
) : IdempotentCommand<ReorderPostMediaResult>(IdempotencyKey);

public record ReorderPostMediaResult(
    Guid PostId,
    List<Guid> OrderedMediaIds,
    DateTimeOffset UpdatedAt
);
