using BuildingBlocks.Exceptions;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wellness.API.Common.Authentication;
using Wellness.Application.Features.JournalMoods.Queries;

namespace Wellness.API.Endpoints.JournalMoods;

public record GetHasJournalMoodStatusResponse(bool HasMood);

public class GetHasJournalMoodStatusEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/me/journal-moods/status", async (
            ISender sender,
            ICurrentActorAccessor currentActor
        ) =>
        {
            var subjectRef = currentActor.GetRequiredSubjectRef();

            var query = new GetHasJournalMoodStatusQuery(
                SubjectRef: subjectRef
            );

            var result = await sender.Send(query);

            return Results.Ok(new GetHasJournalMoodStatusResponse(result.HasMood));
        })
        .RequireAuthorization()
        .WithName("GetHasJournalMoodStatus")
        .WithTags("JournalMoods")
        .Produces<GetHasJournalMoodStatusResponse>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Check if current user has a journal mood entry for today")
        .WithDescription("Check if current user has a journal mood entry for today (Vietnam time)");
    }
}