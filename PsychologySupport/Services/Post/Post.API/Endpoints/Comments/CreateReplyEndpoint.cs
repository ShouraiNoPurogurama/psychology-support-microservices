using Carter;
using Mapster;
using MediatR;
using Post.Application.Features.Comments.Commands.CreateComment;

namespace Post.API.Endpoints.Comments;

public sealed record CreateReplyRequest(
    Guid PostId,
    string Content
);

public sealed record CreateReplyResponse(
    Guid CommentId,
    Guid PostId,
    string Content,
    Guid? ParentCommentId,
    int Level,
    DateTimeOffset CreatedAt);

public class CreateReplyEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/comments/{commentId:guid}/replies", async (
                Guid commentId,
                CreateReplyRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new CreateCommentCommand(
                    request.PostId,
                    request.Content,
                    commentId 
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<CreateReplyResponse>();

                return Results.Created($"/api/comments/{response.CommentId}", response);
            })
            .RequireAuthorization()
            .WithTags("Comments")
            .WithName("CreateReply")
            .WithSummary("Creates a reply to an existing comment.")
            .WithDescription("This endpoint allows an authenticated user to reply to an existing comment on a post. The 'Content' field is required. The parent comment is specified by the commentId route parameter. Replies are subject to moderation and nesting rules.")
            .Produces<CreateReplyResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}