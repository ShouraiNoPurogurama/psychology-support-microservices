using BuildingBlocks.Enums;
using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features.TherapeuticActivity.GetAllTherapeuticActivity;

public record GetAllTherapeuticActivitiesRequest(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = null,
    IntensityLevel? IntensityLevel = null,
    ImpactLevel? ImpactLevel = null
);
public record GetAllTherapeuticActivitiesResponse(PaginatedResult<TherapeuticActivityDto> TherapeuticActivities);

public class GetAllTherapeuticActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/therapeutic-activities", async ([AsParameters] GetAllTherapeuticActivitiesRequest request, ISender sender) =>
        {
            var query = request.Adapt<GetAllTherapeuticActivitiesQuery>();

            var result = await sender.Send(query);

            var response = result.Adapt<GetAllTherapeuticActivitiesResponse>();

            return Results.Ok(response);
        })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllTherapeuticActivities")
            .Produces<GetAllTherapeuticActivitiesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get all therapeutic activities with pagination")
            .WithSummary("Get Paginated Therapeutic Activities");
    }
}