using Post.Application.Aggregates.Posts.Commands.UpdatePostVisibility;
using Post.Domain.Aggregates.Posts.Enums;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;

namespace Post.API.Endpoints.Posts;

public sealed record UpdatePostVisibilityRequest(
    PostVisibility Visibility
);

public sealed record UpdatePostVisibilityResponse(
    Guid PostId,
    string Visibility,
    DateTimeOffset UpdatedAt
);

public class UpdatePostVisibilityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/v1/posts/{postId:guid}/visibility", async (
                Guid postId,
                UpdatePostVisibilityRequest request,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                ILogger<UpdatePostVisibilityEndpoint> logger,
                CancellationToken ct) =>
            {
                if (requestKey is null || requestKey == Guid.Empty)
                {
                    logger.LogWarning("Missing or invalid Idempotency-Key header.");
                    throw new BadRequestException("Đã có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.",
                        "MISSING_IDEMPOTENCY_KEY");
                }

                var command = new UpdatePostVisibilityCommand(
                    requestKey.Value,
                    postId,
                    request.Visibility
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<UpdatePostVisibilityResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("UpdatePostVisibility")
            .WithSummary("Changes the visibility of a post, controlling who can access it.")
            .WithDescription("This endpoint allows authorized users to update the visibility of a post (e.g., public, private, restricted). The 'Idempotency-Key' header ensures that repeated requests do not result in duplicate updates. The postId must refer to an existing post. The response includes the post ID, new visibility, and update timestamp. Returns 400 for missing/invalid idempotency key, 401/403 for unauthorized access, 404 if post not found.")
            .Produces<UpdatePostVisibilityResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
