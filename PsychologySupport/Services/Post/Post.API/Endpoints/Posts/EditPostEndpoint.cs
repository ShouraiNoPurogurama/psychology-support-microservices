using Post.Application.Aggregates.Posts.Commands.EditPost;
using Carter;
using Mapster;
using MediatR;

namespace Post.API.Endpoints.Posts;

public sealed record EditPostRequest(
    string Content,
    string? Title = null,
    List<string>? MediaUrls = null,
    List<Guid>? CategoryTagIds = null
);

public sealed record EditPostResponse(
    Guid PostId,
    DateTimeOffset UpdatedAt
);

public class EditPostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/v1/posts/{postId:guid}", async (
                Guid postId,
                EditPostRequest request,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new EditPostCommand(
                    Guid.Empty, // AliasId should come from auth context
                    postId,
                    request.Content,
                    request.Title,
                    request.MediaUrls,
                    request.CategoryTagIds
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<EditPostResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("EditPost")
            .WithSummary("Edits an existing user post.")
            .WithDescription("This endpoint updates the content, title, media, and category tags of an existing post. The postId must refer to a post owned by the authenticated user. All changes are subject to moderation and business rules. Title is optional; content is required.")
            .Produces<EditPostResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
