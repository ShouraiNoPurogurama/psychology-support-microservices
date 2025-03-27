using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features.PhysicalActivity.GetAllPhysicalActivity;

public record GetAllPhysicalActivitiesResponse(PaginatedResult<PhysicalActivityDto> PhysicalActivities);

public class GetAllPhysicalActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/physical-activities", async ([AsParameters] GetAllPhysicalActivitiesQuery request, ISender sender) =>
        {
            var result = await sender.Send(request);
            var response = result.Adapt<GetAllPhysicalActivitiesResponse>();

            return Results.Ok(response);
        })
            .WithName("GetAllPhysicalActivities")
            .Produces<GetAllPhysicalActivitiesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get All PhysicalActivities")
            .WithSummary("Get All PhysicalActivities");
    }
}