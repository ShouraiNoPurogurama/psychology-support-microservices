using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;
using Post.Application.Features.Posts.Commands.SetPostCoverMedia;

namespace Post.API.Endpoints.Posts;

public sealed record SetPostCoverMediaRequest(
    Guid MediaId
);

public sealed record SetPostCoverMediaResponse(
    Guid PostId,
    Guid CoverMediaId,
    DateTimeOffset UpdatedAt
);

public class SetPostCoverMediaEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/v1/posts/{postId:guid}/cover-media", async (
                Guid postId,
                SetPostCoverMediaRequest request,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                ILogger<SetPostCoverMediaEndpoint> logger,
                CancellationToken ct) =>
            {
                if (requestKey is null || requestKey == Guid.Empty)
                {
                    logger.LogWarning("Missing or invalid Idempotency-Key header.");
                    throw new BadRequestException("Đã có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.",
                        "MISSING_IDEMPOTENCY_KEY");
                }

                var command = new SetPostCoverMediaCommand(
                    requestKey.Value,
                    postId,
                    request.MediaId
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<SetPostCoverMediaResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("SetPostCoverMedia")
            .WithSummary("Sets or updates the cover media for a post, ensuring idempotency and proper authorization.")
            .WithDescription("This endpoint assigns a media item as the cover for the specified post. The 'Idempotency-Key' header ensures that repeated requests do not result in duplicate updates. The postId and mediaId must refer to existing resources. Only authorized users (post owners or admins) can set cover media. The response includes the post ID, cover media ID, and update timestamp. Returns 400 for missing/invalid idempotency key, 401/403 for unauthorized access, 404 if post or media not found.")
            .Produces<SetPostCoverMediaResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
