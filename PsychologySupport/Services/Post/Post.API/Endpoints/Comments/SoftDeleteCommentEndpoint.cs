using Carter;
using Mapster;
using MediatR;
using Post.Application.Features.Comments.Commands.SoftDeleteComment;

namespace Post.API.Endpoints.Comments;

public sealed record SoftDeleteCommentResponse(
    Guid CommentId,
    DateTimeOffset DeletedAt
);

/// <summary>
/// Endpoint for soft-deleting a comment.
/// </summary>
public class SoftDeleteCommentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/v1/comments/{commentId:guid}", async (
                Guid commentId,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new SoftDeleteCommentCommand(
                    Guid.NewGuid(), // IdempotencyKey
                    commentId
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<SoftDeleteCommentResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Comments")
            .WithName("SoftDeleteComment")
            .WithSummary("Soft deletes a comment.")
            .WithDescription("This endpoint marks a comment as deleted without permanently removing it. Only the comment author can delete their comment. The endpoint publishes an integration event for downstream services.")
            .Produces<SoftDeleteCommentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
