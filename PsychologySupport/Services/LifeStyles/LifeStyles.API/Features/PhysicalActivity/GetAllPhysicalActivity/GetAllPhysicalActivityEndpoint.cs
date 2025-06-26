using BuildingBlocks.Enums;
using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features.PhysicalActivity.GetAllPhysicalActivity;

public record GetAllPhysicalActivitiesRequest(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = null,
    IntensityLevel? IntensityLevel = null,
    ImpactLevel? ImpactLevel = null
);

public record GetAllPhysicalActivitiesResponse(PaginatedResult<PhysicalActivityDto> PhysicalActivities);

public class GetAllPhysicalActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/physical-activities", async ([AsParameters] GetAllPhysicalActivitiesRequest request, ISender sender) =>
        {
            var query = request.Adapt<GetAllPhysicalActivitiesQuery>();
            var result = await sender.Send(query);
            var response = result.Adapt<GetAllPhysicalActivitiesResponse>();

            return Results.Ok(response);
        })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllPhysicalActivities")
            .Produces<GetAllPhysicalActivitiesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get All PhysicalActivities")
            .WithSummary("Get All PhysicalActivities");
    }
}