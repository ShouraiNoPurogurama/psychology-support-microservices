using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Posts.Commands.RejectPost;

public record RejectPostCommand(
    Guid IdempotencyKey,
    Guid PostId,
    string Reason
) : IdempotentCommand<RejectPostResult>(IdempotencyKey);

public record RejectPostResult(
    Guid PostId,
    string ModerationStatus,
    string Reason,
    DateTimeOffset RejectedAt
);
