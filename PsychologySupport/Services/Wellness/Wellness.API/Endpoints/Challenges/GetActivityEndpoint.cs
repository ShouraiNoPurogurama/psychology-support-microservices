using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wellness.Application.Features.Challenges.Dtos;
using Wellness.Application.Features.Challenges.Queries;
using Wellness.Domain.Aggregates.Challenges.Enums;

namespace Wellness.API.Endpoints.Challenges;

public record GetActivitiesRequest(
    ActivityType? ActivityType,
    int PageIndex = 1,
    int PageSize = 10
);

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
                request.PageIndex,
                request.PageSize
            );

            var result = await sender.Send(query);

            return Results.Ok(new GetActivitiesResponse(result.Activities));
        })
        .RequireAuthorization()
        .WithName("GetActivities")
        .WithTags("Activities")
        .Produces<GetActivitiesResponse>(200)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get paginated Activities")
        .WithDescription("Returns paginated list of activities filtered by ActivityType, sorted by CreatedAt desc.");
    }
}
