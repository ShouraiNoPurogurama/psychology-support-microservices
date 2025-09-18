using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;
using Post.Application.Features.Posts.Commands.ReportPost;

namespace Post.API.Endpoints.Posts;

public sealed record ReportPostRequest(
    string Reason
);

public sealed record ReportPostResponse(
    Guid ReportId,
    Guid PostId,
    string Reason,
    DateTimeOffset ReportedAt
);

public class ReportPostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/posts/{postId:guid}/reports", async (
                Guid postId,
                ReportPostRequest request,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                ILogger<ReportPostEndpoint> logger,
                CancellationToken ct) =>
            {
                if (requestKey is null || requestKey == Guid.Empty)
                {
                    logger.LogWarning("Missing or invalid Idempotency-Key header.");
                    throw new BadRequestException("Đã có lỗi xảy ra khi xử lý yêu cầu. Vui lòng thử lại sau.",
                        "MISSING_IDEMPOTENCY_KEY");
                }

                var command = new ReportPostCommand(
                    requestKey.Value,
                    postId,
                    request.Reason
                );

                var result = await sender.Send(command, ct);

                var response = result.Adapt<ReportPostResponse>();

                return Results.Created($"/v1/reports/{response.ReportId}", response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithName("ReportPost")
            .WithSummary("Submits a report for a post suspected of violating community guidelines, with idempotency and traceability.")
            .WithDescription("This endpoint allows users to report a post for violations, specifying a reason. The 'Idempotency-Key' header ensures that duplicate reports are not created. The postId must refer to an existing post. The response includes the report ID, post ID, reason, and timestamp. Only authenticated users can report posts. Returns 400 for missing/invalid idempotency key, 401 for unauthorized access, 404 if post not found.")
            .Produces<ReportPostResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
