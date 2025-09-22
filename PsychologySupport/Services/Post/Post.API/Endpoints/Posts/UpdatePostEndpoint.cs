using Carter;
using Mapster;
using MediatR;
using Post.Application.Features.Posts.Commands.UpdatePost;

namespace Post.API.Endpoints.Posts;

public sealed record UpdatePostRequest(
    string NewContent,
    string? NewTitle = null,
    Guid? EditorAliasId = null
);

public sealed record UpdatePostResponse(
    Guid PostId,
    DateTimeOffset UpdatedAt
);

public class UpdatePostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/v1/posts/{postId:guid}", async (
                Guid postId,
                UpdatePostRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new UpdatePostCommand(
                    postId,
                    request.NewContent,
                    request.NewTitle,
                    request.EditorAliasId ?? Guid.Empty // This should come from auth context
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<UpdatePostResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("UpdatePost")
            .WithSummary("Updates the content and title of an existing post, recording the editor and update timestamp.")
            .WithDescription("This endpoint allows authorized users to update the content and title of a post. The postId must refer to an existing post. The request may include the editor's alias ID, which is typically resolved from the authentication context. The response includes the post ID and update timestamp. Returns 401/403 for unauthorized access, 404 if post not found.")
            .Produces<UpdatePostResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
