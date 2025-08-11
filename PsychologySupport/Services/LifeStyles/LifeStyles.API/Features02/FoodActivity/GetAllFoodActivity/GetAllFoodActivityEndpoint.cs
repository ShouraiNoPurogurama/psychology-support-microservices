using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features02.FoodActivity.GetAllFoodActivity;

public record GetAllFoodActivitiesV2Request(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = null
);

public record GetAllFoodActivitiesV2Response(PaginatedResult<FoodActivityDto> FoodActivities);

public class GetAllFoodActivitiesV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("v2/foodActivities", async (
            [AsParameters] GetAllFoodActivitiesV2Request request,
            ISender sender,
            HttpContext httpContext) =>
        {
            if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden",
                    detail: "You do not have permission to access this resource."
                );

            var query = request.Adapt<GetAllFoodActivitiesV2Query>();
            var result = await sender.Send(query);

            var response = new GetAllFoodActivitiesV2Response(result.FoodActivities);
            return Results.Ok(response);
        })
        .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
        .WithName("GetAllFoodActivities v2")
        .WithTags("FoodActivities Version 2")
        .Produces<GetAllFoodActivitiesV2Response>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Get All Food Activities")
        .WithSummary("Get All Food Activities");
    }
}