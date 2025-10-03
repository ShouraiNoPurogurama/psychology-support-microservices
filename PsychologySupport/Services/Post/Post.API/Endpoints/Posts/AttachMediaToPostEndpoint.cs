using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;
using Post.Application.Features.Posts.Commands.AttachMediaToPost;
using Post.Application.Features.Posts.Dtos;

namespace Post.API.Endpoints.Posts;

public sealed record AttachMediaToPostRequest(
    Guid MediaId,
    string MediaUrl,
    int? Position = null
);

public sealed record AttachMediaToPostResponse(
    Guid PostId,
    Guid MediaId,
    DateTimeOffset AttachedAt
);

public class AttachMediaToPostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/posts/{postId:guid}/media", async (
                Guid postId,
                AttachMediaToPostRequest request,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                ILogger<AttachMediaToPostEndpoint> logger,
                CancellationToken ct) =>
            {
                if (requestKey is null || requestKey == Guid.Empty)
                {
                    logger.LogWarning("Missing or invalid Idempotency-Key header.");
                    throw new BadRequestException("Đã có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.",
                        "MISSING_IDEMPOTENCY_KEY");
                }

                var command = new AttachMediaToPostCommand(
                    requestKey.Value,
                    postId,
                    new MediaItemDto(request.MediaId, request.MediaUrl),
                    request.Position
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<AttachMediaToPostResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("AttachMediaToPost")
            .WithSummary("Attaches a media item to a user post.")
            .WithDescription("This endpoint attaches a media item to the specified post. The 'Idempotency-Key' header is required for idempotent attachment. The media must be owned by the user and not already attached. The position is optional and determines the order of media in the post.")
            .Produces<AttachMediaToPostResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
