using Post.Application.Aggregates.Posts.Commands.UnpublishPost;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;

namespace Post.API.Endpoints.Posts;

public sealed record UnpublishPostResponse(
    Guid PostId,
    string Status,
    DateTimeOffset UnpublishedAt
);

public class UnpublishPostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/posts/{postId:guid}/unpublish", async (
                Guid postId,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                ILogger<UnpublishPostEndpoint> logger,
                CancellationToken ct) =>
            {
                if (requestKey is null || requestKey == Guid.Empty)
                {
                    logger.LogWarning("Missing or invalid Idempotency-Key header.");
                    throw new BadRequestException("Đã có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.",
                        "MISSING_IDEMPOTENCY_KEY");
                }

                var command = new UnpublishPostCommand(
                    requestKey.Value,
                    postId
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<UnpublishPostResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("UnpublishPost")
            .WithSummary("Unpublishes a post, making it inaccessible to users while retaining its data for future actions.")
            .WithDescription("This endpoint marks a post as unpublished, updating its status and timestamp. The 'Idempotency-Key' header ensures that repeated requests do not result in multiple unpublishing actions. The postId must refer to an existing post. Only authorized users (owners or admins) can unpublish posts. The response includes the post ID, new status, and unpublish timestamp. Returns 400 for missing/invalid idempotency key, 401/403 for unauthorized access, 404 if post not found.")
            .Produces<UnpublishPostResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
