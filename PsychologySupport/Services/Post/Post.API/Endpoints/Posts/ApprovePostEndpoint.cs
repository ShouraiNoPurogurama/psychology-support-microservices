using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;
using Post.Application.Features.Posts.Commands.ApprovePost;

namespace Post.API.Endpoints.Posts;

public sealed record ApprovePostResponse(
    Guid PostId,
    string ModerationStatus,
    DateTimeOffset ApprovedAt
);

public class ApprovePostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/v1/posts/{postId:guid}/approve", async (
                [FromRoute] Guid postId,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                ILogger<ApprovePostEndpoint> logger,
                CancellationToken ct) =>
            {
                if (requestKey is null || requestKey == Guid.Empty)
                {
                    logger.LogWarning("Missing or invalid Idempotency-Key header.");
                    throw new BadRequestException("Đã có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.",
                        "MISSING_IDEMPOTENCY_KEY");
                }

                var command = new ApprovePostCommand(
                    requestKey.Value,
                    postId
                );

                var result = await sender.Send(command, ct);
                var response = result.Adapt<ApprovePostResponse>();
                return Results.Ok(response);
            })
            .RequireAuthorization("ContentModerator")
            .WithTags("Posts")
            .WithName("ApprovePost")
            .WithSummary("Approves a post for publication.")
            .WithDescription("This endpoint allows a content moderator to approve a post for publication. The 'Idempotency-Key' header is required for idempotent approval. Only users with the 'ContentModerator' role can access this endpoint. The postId must refer to a post pending moderation.")
            .Produces<ApprovePostResponse>(StatusCodes.Status200OK);
    }
}
