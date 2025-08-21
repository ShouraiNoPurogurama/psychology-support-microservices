using BuildingBlocks.Enums;
using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features02.PhysicalActivity.GetAllPhysicalActivity;

public record GetAllPhysicalActivitiesV2Request(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = null,
    IntensityLevel? IntensityLevel = null,
    ImpactLevel? ImpactLevel = null
);

public record GetAllPhysicalActivitiesV2Response(PaginatedResult<PhysicalActivityDto> PhysicalActivities);

// Carter endpoint module
public class GetAllPhysicalActivitiesV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("v2/activities/physical",
            async ([AsParameters] GetAllPhysicalActivitiesV2Request request, ISender sender) =>
            {
                var query = request.Adapt<GetAllPhysicalActivitiesV2Query>();
                var result = await sender.Send(query);
                var response = result.Adapt<GetAllPhysicalActivitiesV2Response>();

                return Results.Ok(response);
            })
        .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
        .WithTags("PhysicalActivities Version 2")
        .WithName("GetAllPhysicalActivities v2")
        .Produces<GetAllPhysicalActivitiesV2Response>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Get All PhysicalActivities")
        .WithSummary("Get All PhysicalActivities");
    }
}