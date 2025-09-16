using Post.Application.Aggregates.Comments.Commands.CreateComment; // ...ở đây
using Carter;
using Mapster;
using MediatR;

namespace Post.API.Endpoints.Comments;

public sealed record CreateCommentRequest(
    Guid PostId,
    string Content,
    Guid? ParentCommentId = null
);

public sealed record CreateCommentResponse(Guid CommentId);

public class CreateCommentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/comments", async (
                CreateCommentRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new CreateCommentCommand(
                    request.PostId,
                    request.Content,
                    request.ParentCommentId
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<CreateCommentResponse>();

                return Results.Created($"/v1/comments/{response.CommentId}", response);
            })
            .RequireAuthorization()
            .WithTags("Comments")
            .WithName("CreateComment")
            .WithSummary("Creates a new comment on a post.")
            .WithDescription("This endpoint allows an authenticated user to create a comment on a post. The 'Content' field is required. Optionally, a parentCommentId can be provided to create a reply. The comment will be associated with the specified post and subject to moderation policies.")
            .Produces<CreateCommentResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}