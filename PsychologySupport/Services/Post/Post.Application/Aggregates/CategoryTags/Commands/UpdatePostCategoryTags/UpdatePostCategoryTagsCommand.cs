using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.CategoryTags.Commands.UpdatePostCategoryTags;

public record UpdatePostCategoryTagsCommand(
    Guid IdempotencyKey,
    Guid PostId,
    IEnumerable<Guid> CategoryTagIds
) : IdempotentCommand<UpdatePostCategoryTagsResult>(IdempotencyKey);

public record UpdatePostCategoryTagsResult(
    Guid PostId,
    IEnumerable<Guid> AddedCategoryTagIds,
    IEnumerable<Guid> RemovedCategoryTagIds,
    DateTimeOffset UpdatedAt
);
