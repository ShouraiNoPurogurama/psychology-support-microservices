using Post.Application.Aggregates.Posts.Commands.UpdateMediaAltText;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;

namespace Post.API.Endpoints.Posts;

public sealed record UpdateMediaAltTextRequest(
    string AltText
);

public sealed record UpdateMediaAltTextResponse(
    Guid MediaId,
    string AltText,
    DateTimeOffset UpdatedAt
);

public class UpdateMediaAltTextEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/v1/posts/media/{mediaId:guid}/alt-text", async (
                Guid mediaId,
                UpdateMediaAltTextRequest request,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                ILogger<UpdateMediaAltTextEndpoint> logger,
                CancellationToken ct) =>
            {
                if (requestKey is null || requestKey == Guid.Empty)
                {
                    logger.LogWarning("Missing or invalid Idempotency-Key header.");
                    throw new BadRequestException("Đã có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.",
                        "MISSING_IDEMPOTENCY_KEY");
                }

                var command = new UpdateMediaAltTextCommand(
                    requestKey.Value,
                    mediaId,
                    request.AltText
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<UpdateMediaAltTextResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("UpdateMediaAltText")
            .WithSummary("Updates the alt text for a media item in a post, improving accessibility and SEO.")
            .WithDescription("This endpoint allows authorized users to update the alt text of a media item attached to a post. The 'Idempotency-Key' header ensures that repeated requests do not result in duplicate updates. The mediaId must refer to an existing media item. The response includes the media ID, new alt text, and update timestamp. Returns 400 for missing/invalid idempotency key, 401/403 for unauthorized access, 404 if media not found.")
            .Produces<UpdateMediaAltTextResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
