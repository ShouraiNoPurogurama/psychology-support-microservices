using BuildingBlocks.Enums;
using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features02.EntertainmentActivity.GetAllEntertainmentActivity;

public record GetAllEntertainmentActivitiesV2Request(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = null,
    IntensityLevel? IntensityLevel = null,
    ImpactLevel? ImpactLevel = null
);

public record GetAllEntertainmentActivitiesV2Response(
    PaginatedResult<EntertainmentActivityDto> EntertainmentActivities
);

public class GetAllEntertainmentActivityV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("v2/entertainmentActivities", async (
            [AsParameters] GetAllEntertainmentActivitiesV2Request request,
            ISender sender, HttpContext httpContext) =>
        {
            // Authorization check
            if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden",
                    detail: "You do not have permission to access this resource."
                );

            var query = request.Adapt<GetAllEntertainmentActivitiesV2Query>();

            var result = await sender.Send(query);
            var response = new GetAllEntertainmentActivitiesV2Response(result.EntertainmentActivities);

            return Results.Ok(response);
        })
        .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
        .WithName("GetAllEntertainmentActivities Version 2")
        .WithTags("EntertainmentActivities Version 2")
        .Produces<GetAllEntertainmentActivitiesV2Result>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Get All Entertainment Activities")
        .WithSummary("Get All Entertainment Activities");
    }
}
