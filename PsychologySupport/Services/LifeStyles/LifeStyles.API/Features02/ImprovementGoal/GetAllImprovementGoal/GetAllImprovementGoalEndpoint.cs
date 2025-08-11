using Carter;
using LifeStyles.API.Dtos;
using Mapster;
using MediatR;

namespace LifeStyles.API.Features02.ImprovementGoal.GetAllImprovementGoal;

public record GetAllImprovementGoalV2Response(IEnumerable<ImprovementGoalDto> Goals);
public class GetAllImprovementGoalV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v2/improvementGoals", async (ISender sender) =>
        {
            var result = await sender.Send(new GetAllImprovementGoalQuery());

            var response = result.Adapt<GetAllImprovementGoalV2Response>();

            return Results.Ok(response);
        })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllImprovementGoals v2")
            .WithTags("ImprovementGoals  Version 2")
            .WithSummary("Get all improvement goals")
            .WithDescription("Returns a list of all improvement goals")
            .Produces<GetAllImprovementGoalV2Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
