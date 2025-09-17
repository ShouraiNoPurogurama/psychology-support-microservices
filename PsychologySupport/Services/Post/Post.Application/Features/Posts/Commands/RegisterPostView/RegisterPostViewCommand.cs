using BuildingBlocks.CQRS;

namespace Post.Application.Features.Posts.Commands.RegisterPostView;

public record RegisterPostViewCommand(
    Guid IdempotencyKey,
    Guid PostId
) : IdempotentCommand<RegisterPostViewResult>(IdempotencyKey);

public record RegisterPostViewResult(
    Guid PostId,
    int NewViewCount,
    DateTimeOffset ViewedAt
);
