using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features.FoodActivity.GetAllFoodActivity;

public record GetAllFoodActivitiesRequest(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = null
);
public record GetAllFoodActivitiesResponse(PaginatedResult<FoodActivityDto> FoodActivities);

public class GetAllFoodActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/food-activities", async (
             [AsParameters] GetAllFoodActivitiesRequest request,
             ISender sender, HttpContext httpContext) =>
        {
            // Authorization check
            if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                return Results.Problem(
                           statusCode: StatusCodes.Status403Forbidden,
                           title: "Forbidden",
                           detail: "You do not have permission to access this resource."
                       );

            var query = request.Adapt<GetAllFoodActivitiesQuery>();
            var result = await sender.Send(query);
            var response = new GetAllFoodActivitiesResponse(result.FoodActivities);

            return Results.Ok(response);
        })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllFoodActivities")
            .WithTags("FoodActivities")
            .Produces<GetAllFoodActivitiesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("GetAllFoodActivities")
            .WithSummary("Get All FoodActivities");
    }
}