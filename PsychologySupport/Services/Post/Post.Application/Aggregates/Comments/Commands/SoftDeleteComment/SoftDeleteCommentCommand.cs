using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Comments.Commands.SoftDeleteComment;

public record SoftDeleteCommentCommand(
    Guid IdempotencyKey,
    Guid CommentId
) : IdempotentCommand<SoftDeleteCommentResult>(IdempotencyKey);

public record SoftDeleteCommentResult(
    Guid CommentId,
    DateTimeOffset DeletedAt
);
