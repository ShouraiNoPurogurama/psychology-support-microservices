using Post.Domain.Aggregates.Posts.Enums;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;
using Post.Application.Features.Posts.Commands.PublishPost;

namespace Post.API.Endpoints.Posts;

public sealed record PublishPostResponse(
    Guid PostId,
    PostVisibility Visibility,
    DateTimeOffset PublishedAt
);

public class PublishPostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/posts/{postId:guid}/publish", async (
                Guid postId,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                ILogger<PublishPostEndpoint> logger,
                CancellationToken ct) =>
            {
                if (requestKey is null || requestKey == Guid.Empty)
                {
                    logger.LogWarning("Missing or invalid Idempotency-Key header.");
                    throw new BadRequestException("Đã có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.",
                        "MISSING_IDEMPOTENCY_KEY");
                }

                var command = new PublishPostCommand(
                    requestKey.Value,
                    postId
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<PublishPostResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("PublishPost")
            .WithSummary("Publishes a user post and makes it visible to others.")
            .WithDescription("This endpoint allows the owner of a post to publish it, making it visible according to the specified visibility. The 'Idempotency-Key' header is required for idempotent publishing. The post must be eligible for publishing and not already published. Only the owner or an admin can publish a post.")
            .Produces<PublishPostResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
