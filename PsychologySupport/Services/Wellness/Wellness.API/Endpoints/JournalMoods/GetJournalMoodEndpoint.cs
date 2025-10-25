using BuildingBlocks.Exceptions;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wellness.API.Common.Authentication;
using Wellness.Application.Features.JournalMoods.Dtos;
using Wellness.Application.Features.JournalMoods.Queries;

namespace Wellness.API.Endpoints.JournalMoods;

public record GetJournalMoodsRequest(
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate
);

public record GetJournalMoodsResponse(IReadOnlyList<JournalMoodDto> Moods);

public class GetJournalMoodEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/me/journal-moods", async (
            [AsParameters] GetJournalMoodsRequest request,
            ISender sender,
            ICurrentActorAccessor currentActor
        ) =>
        {
            var subjectRef = currentActor.GetRequiredSubjectRef();

            var query = new GetJournalMoodsQuery(
                SubjectRef: subjectRef,
                StartDate: request.StartDate,
                EndDate: request.EndDate
            );

            var result = await sender.Send(query);

            return Results.Ok(new GetJournalMoodsResponse(result.Moods));
        })
        .RequireAuthorization()
        .WithName("GetJournalMoods")
        .WithTags("JournalMoods")
        .Produces<GetJournalMoodsResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get JournalMoods for current user")
        .WithDescription("Get JournalMoods for current user");
    }
}
