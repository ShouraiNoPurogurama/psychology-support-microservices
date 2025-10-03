using BuildingBlocks.CQRS;
using Post.Application.Features.Comments.Dtos;

namespace Post.Application.Features.Comments.Queries.GetCommentById;

public record GetCommentByIdQuery(
    Guid CommentId
) : IQuery<GetCommentByIdResult>;

public record GetCommentByIdResult(
    CommentSummaryDto? Comment
);
