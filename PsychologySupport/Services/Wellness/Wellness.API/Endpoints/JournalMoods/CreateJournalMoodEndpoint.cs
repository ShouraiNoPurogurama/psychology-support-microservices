using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wellness.API.Common.Authentication;
using Wellness.Application.Features.JournalMoods.Commands;

namespace Wellness.API.Endpoints.JournalMoods;

public record CreateJournalMoodRequest(
    Guid MoodId,
    string? Note
);

public record CreateJournalMoodResponse(
    Guid JournalMoodId,
    Guid SubjectRef,
    Guid MoodId,
    string? Note
);

public class CreateJournalMoodEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/me/journal-moods", async (
            [FromBody] CreateJournalMoodRequest request,
            [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
            ICurrentActorAccessor currentActor,
            ISender sender
        ) =>
        {
            if (requestKey is null || requestKey == Guid.Empty)
                throw new BadRequestException(
                    "Missing or invalid Idempotency-Key header.",
                    "MISSING_IDEMPOTENCY_KEY"
                );

            var subjectRef = currentActor.GetRequiredSubjectRef();

            var command = new CreateJournalMoodCommand(
                IdempotencyKey: requestKey.Value,
                SubjectRef: subjectRef,
                MoodId: request.MoodId,
                Note: request.Note
            );

            var result = await sender.Send(command);

            var response = new CreateJournalMoodResponse(
                result.JournalMoodId,
                result.SubjectRef,
                result.MoodId,
                result.Note
            );

            return Results.Created(
                $"/v1/me/journal-moods/{response.JournalMoodId}",
                response
            );
        })
        .RequireAuthorization()
        .WithName("CreateJournalMood")
        .WithTags("JournalMoods")
        .Produces<CreateJournalMoodResponse>(201)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Create Journal Mood")
        .WithDescription("Create a new journal mood entry for the current user.");
    }
}
