using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wellness.Application.Features.Challenges.Commands;
using BuildingBlocks.Exceptions;

namespace Wellness.API.Endpoints.Challenges;

public record CreateChallengeProgressRequest(
    Guid SubjectRef,
    Guid ChallengeId
);

public record CreateChallengeProgressResponse(
    Guid ChallengeProgressId,
    string Status,
    int ProgressPercent
);

public class CreateChallengeProgressEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/me/challenge-progresses", async (
            [FromBody] CreateChallengeProgressRequest request,
            [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
            ISender sender) =>
        {
            if (requestKey is null || requestKey == Guid.Empty)
                throw new BadRequestException(
                    "Missing or invalid Idempotency-Key header.",
                    "MISSING_IDEMPOTENCY_KEY"
                );

            var command = request.Adapt<CreateChallengeProgressCommand>()
                                 with
            { IdempotencyKey = requestKey.Value };

            var result = await sender.Send(command);

            var response = new CreateChallengeProgressResponse(
                result.ChallengeProgressId,
                result.Status.ToString(),
                result.ProgressPercent
            );

            return Results.Created(
                $"/v1/me/challenge-progresses/{response.ChallengeProgressId}",
                response
            );
        })
        .RequireAuthorization()
        .WithName("CreateChallengeProgress")
        .WithTags("Challenges")
        .Produces<CreateChallengeProgressResponse>(201)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Create Challenge Progress")
        .WithDescription("Initialize a new Challenge Progress for a subject and challenge.");
    }
}
