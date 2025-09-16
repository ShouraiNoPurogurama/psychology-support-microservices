using BuildingBlocks.CQRS;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Aggregates.Posts.Commands.UpdatePostVisibility;

public record UpdatePostVisibilityCommand(
    Guid IdempotencyKey,
    Guid PostId,
    PostVisibility Visibility
) : IdempotentCommand<UpdatePostVisibilityResult>(IdempotencyKey);

public record UpdatePostVisibilityResult(
    Guid PostId,
    string Visibility,
    DateTimeOffset UpdatedAt
);
