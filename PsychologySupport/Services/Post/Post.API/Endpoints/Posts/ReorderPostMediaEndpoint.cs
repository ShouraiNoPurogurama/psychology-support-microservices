using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;
using Post.Application.Features.Posts.Commands.ReorderPostMedia;

namespace Post.API.Endpoints.Posts;

public sealed record ReorderPostMediaRequest(
    List<Guid> OrderedMediaIds
);

public sealed record ReorderPostMediaResponse(
    Guid PostId,
    List<Guid> OrderedMediaIds,
    DateTimeOffset UpdatedAt
);

public class ReorderPostMediaEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/v1/posts/{postId:guid}/media/order", async (
                Guid postId,
                ReorderPostMediaRequest request,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                ILogger<ReorderPostMediaEndpoint> logger,
                CancellationToken ct) =>
            {
                if (requestKey is null || requestKey == Guid.Empty)
                {
                    logger.LogWarning("Missing or invalid Idempotency-Key header.");
                    throw new BadRequestException("Đã có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.",
                        "MISSING_IDEMPOTENCY_KEY");
                }

                var command = new ReorderPostMediaCommand(
                    requestKey.Value,
                    postId,
                    request.OrderedMediaIds
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<ReorderPostMediaResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("ReorderPostMedia")
            .WithSummary("Updates the order of media attachments for a post, ensuring atomic and idempotent reordering.")
            .WithDescription("This endpoint allows users to specify a new order for media items attached to a post. The 'Idempotency-Key' header ensures that repeated requests do not result in inconsistent states. The postId must refer to an existing post, and all media IDs must be valid and attached to the post. Only authorized users can reorder media. The response includes the post ID, new media order, and update timestamp. Returns 400 for missing/invalid idempotency key, 401/403 for unauthorized access, 404 if post or media not found.")
            .Produces<ReorderPostMediaResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
