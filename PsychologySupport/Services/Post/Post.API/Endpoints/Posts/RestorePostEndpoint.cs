using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;
using Post.Application.Features.Posts.Commands.RestorePost;

namespace Post.API.Endpoints.Posts;

public sealed record RestorePostResponse(
    Guid PostId,
    string Status,
    DateTimeOffset RestoredAt
);

public class RestorePostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/posts/{postId:guid}/restore", async (
                Guid postId,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                ILogger<RestorePostEndpoint> logger,
                CancellationToken ct) =>
            {
                if (requestKey is null || requestKey == Guid.Empty)
                {
                    logger.LogWarning("Missing or invalid Idempotency-Key header.");
                    throw new BadRequestException("Đã có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.",
                        "MISSING_IDEMPOTENCY_KEY");
                }

                var command = new RestorePostCommand(
                    requestKey.Value,
                    postId
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<RestorePostResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("RestorePost")
            .WithSummary("Restores a previously deleted post, updating its status and restoration timestamp.")
            .WithDescription("This endpoint restores a post that was previously deleted (soft or hard), making it accessible again. The 'Idempotency-Key' header ensures that repeated requests do not result in multiple restorations. The postId must refer to a deleted post. Only authorized users (owners or admins) can restore posts. The response includes the post ID, new status, and restoration timestamp. Returns 400 for missing/invalid idempotency key, 401/403 for unauthorized access, 404 if post not found.")
            .Produces<RestorePostResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
