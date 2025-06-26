using BuildingBlocks.Enums;
using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features.EntertainmentActivity.GetAllEntertainmentActivity;

public record GetAllEntertainmentActivitiesRequest(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = null,
    IntensityLevel? IntensityLevel = null,
    ImpactLevel? ImpactLevel = null
);
public record GetAllEntertainmentActivitiesResponse(PaginatedResult<EntertainmentActivityDto> EntertainmentActivities);

public class GetAllEntertainmentActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/entertainment-activities", async (
            [AsParameters] GetAllEntertainmentActivitiesRequest request,
            ISender sender) =>
        {
            var query = request.Adapt<GetAllEntertainmentActivitiesQuery>();

            var result = await sender.Send(query);
            var response = new GetAllEntertainmentActivitiesResponse(result.EntertainmentActivities);

            return Results.Ok(response);
        })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllEntertainmentActivities")
            .WithTags("EntertainmentActivities")
            .Produces<GetAllEntertainmentActivitiesResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("GetAll Entertainment Activities")
            .WithSummary("GetAll Entertainment Activities");
    }
}