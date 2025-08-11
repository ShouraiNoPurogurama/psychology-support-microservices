using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LifeStyles.API.Features02.FoodActivity.GetFoodActivity;

public record GetFoodActivityV2Request(Guid Id);

public record GetFoodActivityV2Response(FoodActivityDto FoodActivity);

public class GetFoodActivityV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("v2/foodActivities/{id:guid}", async (Guid id, ISender sender, HttpContext httpContext) =>
        {
            if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden",
                    detail: "You do not have permission to access this resource."
                );

            var query = new GetFoodActivityV2Query(id);
            var result = await sender.Send(query);
            var response = result.Adapt<GetFoodActivityV2Response>();
            return Results.Ok(response);
        })
        .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
        .WithName("GetFoodActivity v2")
        .WithTags("FoodActivities Version 2")
        .Produces<GetFoodActivityV2Response>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Get Food Activity")
        .WithSummary("Get Food Activity");
    }
}