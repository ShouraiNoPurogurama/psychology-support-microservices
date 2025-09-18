using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;
using Post.Application.Features.Posts.Commands.RejectPost;

namespace Post.API.Endpoints.Posts;

public sealed record RejectPostRequest(
    string Reason
);

public sealed record RejectPostResponse(
    Guid PostId,
    string ModerationStatus,
    string Reason,
    DateTimeOffset RejectedAt
);

public class RejectPostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/posts/{postId:guid}/reject", async (
                Guid postId,
                RejectPostRequest request,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                ILogger<RejectPostEndpoint> logger,
                CancellationToken ct) =>
            {
                if (requestKey is null || requestKey == Guid.Empty)
                {
                    logger.LogWarning("Missing or invalid Idempotency-Key header.");
                    throw new BadRequestException("Đã có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.",
                        "MISSING_IDEMPOTENCY_KEY");
                }

                var command = new RejectPostCommand(
                    requestKey.Value,
                    postId,
                    request.Reason
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<RejectPostResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization("ContentModerator")
            .WithTags("Posts")
            .WithName("RejectPost")
            .WithSummary("Rejects a post during moderation, recording the reason and updating its moderation status.")
            .WithDescription("This endpoint allows a content moderator to reject a post, specifying a reason for rejection. The 'Idempotency-Key' header is required for idempotent moderation actions. Only users with the 'ContentModerator' role can access this endpoint. The postId must refer to a post pending moderation. The response includes the post ID, new moderation status, rejection reason, and timestamp. Returns 400 for missing/invalid idempotency key, 401/403 for unauthorized access, 404 if post not found.")
            .Produces<RejectPostResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
