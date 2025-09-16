using Post.Application.Aggregates.Posts.Commands.RemoveMediaFromPost;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;

namespace Post.API.Endpoints.Posts;

public sealed record RemoveMediaFromPostResponse(
    Guid PostId,
    Guid MediaId,
    DateTimeOffset RemovedAt
);

public class RemoveMediaFromPostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/v1/posts/{postId:guid}/media/{mediaId:guid}", async (
                Guid postId,
                Guid mediaId,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                ILogger<RemoveMediaFromPostEndpoint> logger,
                CancellationToken ct) =>
            {
                if (requestKey is null || requestKey == Guid.Empty)
                {
                    logger.LogWarning("Missing or invalid Idempotency-Key header.");
                    throw new BadRequestException("Đã có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.",
                        "MISSING_IDEMPOTENCY_KEY");
                }

                var command = new RemoveMediaFromPostCommand(
                    requestKey.Value,
                    postId,
                    mediaId
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<RemoveMediaFromPostResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("RemoveMediaFromPost")
            .WithSummary("Removes a specific media attachment from a post, ensuring idempotency and authorization.")
            .WithDescription("This endpoint detaches a media item from the specified post. The 'Idempotency-Key' header is required to prevent duplicate removals. The postId and mediaId must refer to existing resources. Only authorized users (post owners or admins) can perform this action. The response includes the post ID, media ID, and removal timestamp. Returns 400 for missing/invalid idempotency key, 401/403 for unauthorized access, 404 if post or media not found.")
            .Produces<RemoveMediaFromPostResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
