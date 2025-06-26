using Carter;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeStyles.API.Features.ImprovementGoal.GetAllImprovementGoal;

public record GetAllImprovementGoalResponse(IEnumerable<ImprovementGoalDto> Goals);
public class GetAllImprovementGoalEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/improvement-goals", async (ISender sender) =>
        {
            var result = await sender.Send(new GetAllImprovementGoalQuery());

            var response = result.Adapt<GetAllImprovementGoalResponse>();

            return Results.Ok(response);
        })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllImprovementGoals")
            .WithTags("ImprovementGoals")
            .WithSummary("Get all improvement goals")
            .WithDescription("Returns a list of all improvement goals")
            .Produces<GetAllImprovementGoalResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
