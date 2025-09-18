using Post.Domain.Aggregates.Posts.Enums;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;
using Post.Application.Features.Posts.Commands.CreatePost;

namespace Post.API.Endpoints.Posts;

public sealed record CreatePostRequest(
    string? Title,
    string Content,
    PostVisibility Visibility,
    IEnumerable<Guid>? MediaIds = null
);

public sealed record CreatePostResponse(
    Guid Id,
    string ModerationStatus,
    DateTimeOffset CreatedAt
);

public class CreatePostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/posts", async (
                CreatePostRequest request,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                ILogger<CreatePostEndpoint> logger,
                CancellationToken ct) =>
            {
                if (requestKey is null || requestKey == Guid.Empty)
                {
                    logger.LogWarning("Missing or invalid Idempotency-Key header.");
                    throw new BadRequestException("Đã có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.",
                        "MISSING_IDEMPOTENCY_KEY");
                }

                var command = new CreatePostCommand(
                    requestKey.Value,
                    request.Title,
                    request.Content,
                    request.Visibility,
                    request.MediaIds
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<CreatePostResponse>();

                return Results.Created($"/v1/posts/{response.Id}", response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("CreatePost")
            .WithSummary("Creates a new user post.")
            .WithDescription("This endpoint creates a new post. The 'Content' field is mandatory. The 'Idempotency-Key' header is required for idempotent creation. Media IDs are optional. The post will be created with the specified visibility and may be subject to moderation.")
            .Produces<CreatePostResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
