using BuildingBlocks.CQRS;

namespace Post.Application.Features.Comments.Commands.ReportComment;

public record ReportCommentCommand(
    Guid IdempotencyKey,
    Guid CommentId,
    string Reason
) : IdempotentCommand<ReportCommentResult>(IdempotencyKey);

public record ReportCommentResult(
    Guid ReportId,
    Guid CommentId,
    string Reason,
    DateTimeOffset ReportedAt
);
