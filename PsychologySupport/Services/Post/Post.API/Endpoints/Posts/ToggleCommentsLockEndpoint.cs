using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;
using Post.Application.Features.Posts.Commands.ToggleCommentsLock;

namespace Post.API.Endpoints.Posts;

public sealed record ToggleCommentsLockRequest(
    bool IsLocked
);

public sealed record ToggleCommentsLockResponse(
    Guid PostId,
    bool IsCommentsLocked,
    DateTimeOffset UpdatedAt
);

public class ToggleCommentsLockEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/v1/posts/{postId:guid}/comments/lock", async (
                Guid postId,
                ToggleCommentsLockRequest request,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                ILogger<ToggleCommentsLockEndpoint> logger,
                CancellationToken ct) =>
            {
                if (requestKey is null || requestKey == Guid.Empty)
                {
                    logger.LogWarning("Missing or invalid Idempotency-Key header.");
                    throw new BadRequestException("Đã có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.",
                        "MISSING_IDEMPOTENCY_KEY");
                }

                var command = new ToggleCommentsLockCommand(
                    requestKey.Value,
                    postId,
                    request.IsLocked
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<ToggleCommentsLockResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("ToggleCommentsLock")
            .WithSummary("Locks or unlocks comments on a post, controlling user interaction and discussion.")
            .WithDescription("This endpoint allows authorized users to lock or unlock comments on a post, preventing or allowing further discussion. The 'Idempotency-Key' header ensures that repeated requests do not result in inconsistent states. The postId must refer to an existing post. The response includes the post ID, new comment lock status, and update timestamp. Returns 400 for missing/invalid idempotency key, 401/403 for unauthorized access, 404 if post not found.")
            .Produces<ToggleCommentsLockResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
