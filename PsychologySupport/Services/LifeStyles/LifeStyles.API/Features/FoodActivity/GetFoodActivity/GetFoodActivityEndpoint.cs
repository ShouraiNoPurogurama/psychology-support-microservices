using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LifeStyles.API.Features.FoodActivity.GetFoodActivity;

public record GetFoodActivityRequest(Guid Id);

public record GetFoodActivityResponse(FoodActivityDto FoodActivity);

public class GetFoodActivityEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/food-activities/{id:guid}", async (Guid id, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    return Results.Problem(
                               statusCode: StatusCodes.Status403Forbidden,
                               title: "Forbidden",
                               detail: "You do not have permission to access this resource."
                           );

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