using BuildingBlocks.CQRS;

namespace Post.Application.Aggregates.Comments.Commands.CreateComment;

public record CreateCommentCommand(
    Guid PostId,
    string Content,
    Guid? ParentCommentId = null
) : ICommand<CreateCommentResult>;

public record CreateCommentResult(
    Guid CommentId,
    Guid PostId,
    string Content,
    Guid? ParentCommentId,
    int Level,
    DateTimeOffset CreatedAt
);
