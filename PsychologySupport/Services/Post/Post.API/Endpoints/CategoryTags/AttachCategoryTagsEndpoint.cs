using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;
using Post.Application.Aggregates.CategoryTags.Commands.AttachCategoryTagsToPost;

namespace Post.API.Endpoints.CategoryTags;

public sealed record AttachCategoryTagsRequest(
    IEnumerable<Guid> CategoryTagIds
);

public sealed record AttachCategoryTagsResponse(
    Guid PostId,
    IEnumerable<Guid> AttachedCategoryTagIds,
    DateTimeOffset AttachedAt
);

public class AttachCategoryTagsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/posts/{postId:guid}/category-tags", async (
                Guid postId,
                AttachCategoryTagsRequest request,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                ILogger<AttachCategoryTagsEndpoint> logger,
                CancellationToken ct) =>
            {
                if (requestKey is null || requestKey == Guid.Empty)
                {
                    logger.LogWarning("Missing or invalid Idempotency-Key header.");
                    throw new BadRequestException("Đã có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.",
                        "MISSING_IDEMPOTENCY_KEY");
                }

                var command = new AttachCategoryTagsToPostCommand(
                    requestKey.Value,
                    postId,
                    request.CategoryTagIds
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<AttachCategoryTagsResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("CategoryTags")
            .WithName("AttachCategoryTagsToPost")
            .WithSummary("Attaches category tags to a post.")
            .WithDescription("This endpoint attaches one or more category tags to the specified post. The 'Idempotency-Key' header is required for idempotent attachment. Only the post owner or an admin can attach tags. Returns the post ID, attached tag IDs, and timestamp.")
            .Produces<AttachCategoryTagsResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
