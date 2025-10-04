using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;
using Post.Application.Features.Posts.Commands.RegisterPostView;

namespace Post.API.Endpoints.Posts;

public sealed record RegisterPostViewsRequest(List<Guid> PostIds);
public sealed record RegisterPostViewsResponse(int PostsUpdatedCount, DateTimeOffset ViewedAt);

public class RegisterPostViewEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/posts/views", async (
                [FromBody] RegisterPostViewsRequest body, 
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
                    body.PostIds
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<RegisterPostViewsResponse>();

                return Results.Ok(response);
            })
            .WithTags("Posts")
            .WithName("RegisterPostViews") // Tên endpoint cũng nên cập nhật
            .WithSummary("Registers multiple views for a batch of posts.")
            .WithDescription("This endpoint records a view for each specified post in the request body...") // Cập nhật mô tả
            .Produces<RegisterPostViewsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}