using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;
using Post.Application.Features.Posts.Commands.RegisterPostView;

namespace Post.API.Endpoints.Posts;

public sealed record RegisterPostViewResponse(
    Guid PostId,
    int NewViewCount,
    DateTimeOffset ViewedAt
);

public class RegisterPostViewEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/posts/{postId:guid}/views", async (
                Guid postId,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                ILogger<RegisterPostViewEndpoint> logger,
                CancellationToken ct) =>
            {
                if (requestKey is null || requestKey == Guid.Empty)
                {
                    logger.LogWarning("Missing or invalid Idempotency-Key header.");
                    throw new BadRequestException("Đã có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.",
                        "MISSING_IDEMPOTENCY_KEY");
                }

                var command = new RegisterPostViewCommand(
                    requestKey.Value,
                    postId
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<RegisterPostViewResponse>();

                return Results.Ok(response);
            })
            .WithTags("Posts")
            .WithName("RegisterPostView")
            .WithSummary("Registers a unique view for a post, incrementing its view count in a consistent, idempotent manner.")
            .WithDescription(
                "This endpoint records a view for the specified post, ensuring that duplicate requests (via the 'Idempotency-Key' header) do not result in multiple increments. The postId must refer to an existing, accessible post. The response includes the updated view count and the timestamp of registration. This operation is subject to business rules regarding post visibility and access. Returns 400 for missing/invalid idempotency key, 404 if post not found.")
            .Produces<RegisterPostViewResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}