using Carter;
using Mapster;
using MediatR;
using Post.Application.Features.Posts.Commands.DeletePost;

namespace Post.API.Endpoints.Posts;

public sealed record DeletePostResponse(
    Guid PostId,
    DateTimeOffset DeletedAt
);

public class DeletePostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/v1/posts/{postId:guid}", async (
                Guid postId,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new DeletePostCommand(postId);

                var result = await sender.Send(command, ct);

                var response = result.Adapt<DeletePostResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("DeletePost")
            .WithSummary("Deletes an existing user post.")
            .WithDescription("This endpoint deletes the specified post. Only the owner or an admin can delete a post. The postId must refer to an existing post. Deletion is subject to business rules and may be soft or hard depending on system configuration.")
            .Produces<DeletePostResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
