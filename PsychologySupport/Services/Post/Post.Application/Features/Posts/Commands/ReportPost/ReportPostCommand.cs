using BuildingBlocks.CQRS;

namespace Post.Application.Features.Posts.Commands.ReportPost;

public record ReportPostCommand(
    Guid IdempotencyKey,
    Guid PostId,
    string Reason
) : IdempotentCommand<ReportPostResult>(IdempotencyKey);

public record ReportPostResult(
    Guid ReportId,
    Guid PostId,
    string Reason,
    DateTimeOffset ReportedAt
);
