using Carter;
using Feed.Application.Features.UserActivity.Commands.RecordSeenPost;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Exceptions;

namespace Feed.API.Features.UserActivity;

public sealed record RecordSeenPostRequest(
    Guid PostId,
    DateOnly? Date = null,
    int DwellTimeMs = 0
);

public sealed record RecordSeenPostResponse(
    bool Success,
    string Message
);

public class RecordSeenPostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/feed/seen", async (
                RecordSeenPostRequest request,
                [FromHeader(Name = "X-Alias-Id")] Guid aliasId,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                ILogger<RecordSeenPostEndpoint> logger,
                CancellationToken ct) =>
            {
                if (aliasId == Guid.Empty)
                {
                    logger.LogWarning("Missing X-Alias-Id header");
                    throw new BadRequestException("Missing X-Alias-Id header", "MISSING_ALIAS_ID");
                }

                if (requestKey is null || requestKey == Guid.Empty)
                {
                    logger.LogWarning("Missing or invalid Idempotency-Key header");
                    throw new BadRequestException("Missing Idempotency-Key header", "MISSING_IDEMPOTENCY_KEY");
                }

                var command = new RecordSeenPostCommand(
                    requestKey.Value,
                    aliasId,
                    request.PostId,
                    request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow),
                    request.DwellTimeMs
                );

                var result = await sender.Send(command, ct);

                var response = new RecordSeenPostResponse(
                    result.Success,
                    result.Success ? "Post view recorded successfully" : "Failed to record post view"
                );

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("UserActivity")
            .WithName("RecordSeenPost")
            .WithSummary("Record that a user has seen a post")
            .WithDescription("Records user activity for feed analytics and impression tracking with TTL-based retention")
            .Produces<RecordSeenPostResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
