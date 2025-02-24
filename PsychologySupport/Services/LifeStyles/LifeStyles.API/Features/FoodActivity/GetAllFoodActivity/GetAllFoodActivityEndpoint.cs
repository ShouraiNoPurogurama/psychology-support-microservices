using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features.FoodActivity.GetAllFoodActivity;

public record GetAllFoodActivitiesResponse(IEnumerable<FoodActivityDto> FoodActivities);

public class GetAllFoodActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/food-activities", async ([AsParameters] PaginationRequest request, ISender sender) =>
        {
            var query = new GetAllFoodActivitiesQuery(request);
            var result = await sender.Send(query);
            var response = result.Adapt<GetAllFoodActivitiesResponse>();

            return Results.Ok(response);
        })
        .WithName("GetAllFoodActivities")
        .Produces<GetAllFoodActivitiesResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Get All Food Activities")
        .WithSummary("Get All Food Activities");
    }
}
