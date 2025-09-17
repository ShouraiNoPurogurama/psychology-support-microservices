using BuildingBlocks.CQRS;
using Post.Application.Aggregates.Comments.Dtos;

namespace Post.Application.Aggregates.Comments.Queries.GetCommentById;

public record GetCommentByIdQuery(
    Guid CommentId
) : IQuery<GetCommentByIdResult>;

public record GetCommentByIdResult(
    CommentDto? Comment
);
