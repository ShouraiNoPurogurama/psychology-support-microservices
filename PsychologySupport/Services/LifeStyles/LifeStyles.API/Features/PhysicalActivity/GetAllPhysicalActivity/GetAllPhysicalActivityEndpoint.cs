using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features.PhysicalActivity.GetAllPhysicalActivity;

public record GetAllPhysicalActivitiesResponse(IEnumerable<PhysicalActivityDto> PhysicalActivities);

public class GetAllPhysicalActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/physical-activities", async ([AsParameters] PaginationRequest request, ISender sender) =>
            {
                var query = new GetAllPhysicalActivitiesQuery(request);
                var result = await sender.Send(query);
                var response = result.Adapt<GetAllPhysicalActivitiesResponse>();

                return Results.Ok(response);
            })
            .WithName("GetAllPhysicalActivities")
            .Produces<GetAllPhysicalActivitiesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("GetAll Physical Activities")
            .WithSummary("GetAll Physical Activities");
    }
}