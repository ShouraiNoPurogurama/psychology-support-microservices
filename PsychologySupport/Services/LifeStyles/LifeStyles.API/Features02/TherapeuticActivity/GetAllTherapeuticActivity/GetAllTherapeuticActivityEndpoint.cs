using BuildingBlocks.Enums;
using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Dtos;
using LifeStyles.API.Features.TherapeuticActivity.GetTherapeuticActivity;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features02.TherapeuticActivity.GetAllTherapeuticActivity;

public record GetAllTherapeuticActivitiesV2Request(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = null,
    IntensityLevel? IntensityLevel = null,
    ImpactLevel? ImpactLevel = null
);

public record GetAllTherapeuticActivitiesV2Response(
    PaginatedResult<TherapeuticActivityDto> TherapeuticActivities
);


public class GetAllTherapeuticActivityV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v2/activities/therapeutic", async ([AsParameters] GetAllTherapeuticActivitiesV2Request request, ISender sender) =>
        {
            var query = new GetAllTherapeuticActivitiesV2Query(
                request.PageIndex,
                request.PageSize,
                request.Search,
                request.IntensityLevel,
                request.ImpactLevel
            );

            var result = await sender.Send(query);

            var response = result.Adapt<GetAllTherapeuticActivitiesV2Response>();

            return Results.Ok(response);
        })
        .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
        .WithTags("TherapeuticActivities Version 2")
        .WithName("GetAllTherapeuticActivities v2")
        .Produces<GetAllTherapeuticActivitiesV2Response>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Get All Therapeutic Activities")
        .WithSummary("Get All Therapeutic Activities ");
    }
}