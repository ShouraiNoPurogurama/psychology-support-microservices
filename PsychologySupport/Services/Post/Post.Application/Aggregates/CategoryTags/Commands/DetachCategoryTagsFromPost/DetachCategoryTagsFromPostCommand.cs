using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.CategoryTags.Commands.DetachCategoryTagsFromPost;

public record DetachCategoryTagsFromPostCommand(
    Guid IdempotencyKey,
    Guid PostId,
    IEnumerable<Guid> CategoryTagIds
) : IdempotentCommand<DetachCategoryTagsFromPostResult>(IdempotencyKey);

public record DetachCategoryTagsFromPostResult(
    Guid PostId,
    IEnumerable<Guid> DetachedCategoryTagIds,
    DateTimeOffset DetachedAt
);
