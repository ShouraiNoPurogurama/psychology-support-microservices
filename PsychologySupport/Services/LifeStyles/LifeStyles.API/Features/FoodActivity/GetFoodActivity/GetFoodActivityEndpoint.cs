using Carter;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features.FoodActivity.GetFoodActivity;

public record GetFoodActivityRequest(Guid Id);

public record GetFoodActivityResponse(FoodActivityDto FoodActivity);

public class GetFoodActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/food-activities/{id:guid}", async (Guid id, ISender sender) =>
            {
                var query = new GetFoodActivityQuery(id);
                var result = await sender.Send(query);
                var response = result.Adapt<GetFoodActivityResponse>();
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetFoodActivity")
            .Produces<GetFoodActivityResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Food Activity")
            .WithSummary("Get Food Activity");
    }
}