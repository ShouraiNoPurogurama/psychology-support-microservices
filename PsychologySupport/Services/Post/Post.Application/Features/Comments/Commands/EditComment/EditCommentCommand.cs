using BuildingBlocks.CQRS;

namespace Post.Application.Features.Comments.Commands.EditComment;

public record EditCommentCommand(
    Guid IdempotencyKey,
    Guid CommentId,
    string Content
) : IdempotentCommand<EditCommentResult>(IdempotencyKey);

public record EditCommentResult(
    Guid CommentId,
    string Content,
    DateTimeOffset EditedAt
);
