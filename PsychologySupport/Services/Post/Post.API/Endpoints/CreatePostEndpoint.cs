using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Post.Application.Aggregates.Posts.Commands.CreatePost;

namespace Post.API.Endpoints;

public sealed record CreatePostRequest(
    string Content,
    string? Title,
    string Visibility,
    IEnumerable<Guid>? MediaIds
);

public sealed record CreatePostResponse(Guid Id, string ModerationStatus, DateTimeOffset CreatedAt);

public class CreatePostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/posts", async (
                CreatePostRequest body,
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
                    body.Title,
                    body.Content,
                    body.Visibility,
                    body.MediaIds
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<CreatePostResponse>();

                return Results.Created($"/v1/posts/{response.Id}", response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .Produces<CreatePostResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }
}