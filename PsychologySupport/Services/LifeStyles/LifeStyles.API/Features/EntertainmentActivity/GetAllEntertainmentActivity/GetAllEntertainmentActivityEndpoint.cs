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
        app.MapGet("/entertainment-activities", async ([AsParameters] PaginationRequest request, ISender sender) =>
        {
            var query = new GetAllEntertainmentActivitiesQuery(request);
            var result = await sender.Send(query);
            var response = result.Adapt<GetAllEntertainmentActivitiesResponse>();

            return Results.Ok(response);
        })
            .WithName("GetAllEntertainmentActivities")
            .Produces<GetAllEntertainmentActivitiesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("GetAll Entertainment Activities")
            .WithSummary("GetAll Entertainment Activities");
    }
}
