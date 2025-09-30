using Carter;
using Mapster;
using MediatR;
using Post.Application.Features.Posts.Commands.AbandonPost;

namespace Post.API.Endpoints.Posts;

public sealed record AbandonPostResponse(
    Guid PostId,
    string Status,
    DateTimeOffset AbandonedAt
);

/// <summary>
/// Internal endpoint for marking abandoned posts.
/// This is typically called by a scheduled job to identify posts stuck in Creating state.
/// </summary>
public class AbandonPostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/posts/{postId:guid}/abandon", async (
                Guid postId,
                ISender sender,
                CancellationToken ct) =>
            {
                var command = new AbandonPostCommand(postId);

                var result = await sender.Send(command, ct);

                var response = result.Adapt<AbandonPostResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("AbandonPost")
            .WithSummary("Marks a post as abandoned and triggers Emo Bot notification.")
            .WithDescription("This endpoint marks posts that were started but never finalized as abandoned. It publishes an integration event that triggers the Emo Bot in the Moderation service to reach out to the author. Only posts in 'Creating' status can be marked as abandoned.")
            .Produces<AbandonPostResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
