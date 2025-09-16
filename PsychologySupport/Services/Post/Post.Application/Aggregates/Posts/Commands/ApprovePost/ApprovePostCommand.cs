using BuildingBlocks.CQRS;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Aggregates.Posts.Commands.ApprovePost;

public record ApprovePostCommand(
    Guid IdempotencyKey,
    Guid PostId
) : IdempotentCommand<ApprovePostResult>(IdempotencyKey);

public record ApprovePostResult(
    Guid PostId,
    ModerationStatus ModerationStatus,
    DateTimeOffset ApprovedAt
);
