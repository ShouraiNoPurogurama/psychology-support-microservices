using BuildingBlocks.CQRS;

namespace Post.Application.Features.CategoryTags.Commands.AttachCategoryTagsToPost;

public record AttachCategoryTagsToPostCommand(
    Guid IdempotencyKey,
    Guid PostId,
    IEnumerable<Guid> CategoryTagIds
) : IdempotentCommand<AttachCategoryTagsToPostResult>(IdempotencyKey);

public record AttachCategoryTagsToPostResult(
    Guid PostId,
    IEnumerable<Guid> AttachedCategoryTagIds,
    DateTimeOffset AttachedAt
);
