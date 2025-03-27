using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features.EntertainmentActivity.GetAllEntertainmentActivity;

public record GetAllEntertainmentActivitiesResponse(PaginatedResult<EntertainmentActivityDto> EntertainmentActivities);

public class GetAllEntertainmentActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/entertainment-activities", async ([AsParameters] GetAllEntertainmentActivitiesQuery request, ISender sender) =>
        {
            var result = await sender.Send(request);
            var response = result.Adapt<GetAllEntertainmentActivitiesResult>();

            return Results.Ok(response);
        })
            .WithName("GetAllEntertainmentActivities")
            .Produces<GetAllEntertainmentActivitiesResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("GetAll Entertainment Activities")
            .WithSummary("GetAll Entertainment Activities");
    }
}