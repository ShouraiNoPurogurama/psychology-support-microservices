using Post.Application.Aggregates.Posts.Commands.SoftDeletePost;
using Carter;
using Mapster;
using MediatR;

namespace Post.API.Endpoints.Posts;

public sealed record SoftDeletePostResponse(
    Guid PostId,
    DateTimeOffset DeletedAt
);

public class SoftDeletePostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/v1/posts/{postId:guid}/soft", async (
                Guid postId,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new SoftDeletePostCommand(
                    postId,
                    Guid.Empty // DeleterAliasId should come from auth context
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<SoftDeletePostResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("SoftDeletePost")
            .WithSummary("Marks a post as deleted without permanently removing it, enabling future restoration.")
            .WithDescription("This endpoint performs a soft delete on the specified post, updating its status and deletion timestamp. The postId must refer to an existing post. Only authorized users (owners or admins) can perform soft deletes. The response includes the post ID and deletion timestamp. Returns 401/403 for unauthorized access, 404 if post not found.")
            .Produces<SoftDeletePostResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
