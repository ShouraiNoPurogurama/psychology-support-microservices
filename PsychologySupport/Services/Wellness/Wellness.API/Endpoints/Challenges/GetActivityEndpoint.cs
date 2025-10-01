using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wellness.API.Common;
using Wellness.Application.Features.Challenges.Dtos;
using Wellness.Application.Features.Challenges.Queries;
using Wellness.Domain.Aggregates.Challenges.Enums;

namespace Wellness.API.Endpoints.Challenges;

public record GetActivitiesRequest(
    ActivityType? ActivityType,
    int PageIndex = 1,
    int PageSize = 10,
    string? TargetLang = null
)
{
    public PaginationRequest ToPaginationRequest() => new(PageIndex, PageSize);
}

public record GetActivitiesResponse(PaginatedResult<ActivityDto> Activities);

public class GetActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/activities", async (
            [AsParameters] GetActivitiesRequest request,
            ISender sender) =>
        {
            var query = new GetActivitiesQuery(
                request.ActivityType,
                request.ToPaginationRequest(),
                request.TargetLang
            );

            var result = await sender.Send(query);

            return Results.Ok(new GetActivitiesResponse(result.Activities));
        })
        .WithName("GetActivities")
        .WithTags("Activities")
        .Produces<GetActivitiesResponse>(200)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get paginated Activities with translation")
        .WithDescription("Returns paginated activities filtered by ActivityType. Name, Description, and ActivityType are translated if TargetLang is provided.");
    }
}