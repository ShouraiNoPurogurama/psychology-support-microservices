using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Dtos;
using LifeStyles.API.Extensions;
using MediatR;

namespace LifeStyles.API.Features02.ImprovementGoal.GetImprovementGoalsWithSelection;


public record GetImprovementGoalsWithSelectionV2Response(PaginatedResult<ImprovementGoalWithSelectionDto> Goals);

public class GetImprovementGoalsWithSelectionV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v2/me/improvementGoals", 
            async ([AsParameters] PaginationRequest paginationRequest, HttpContext httpContext, ISender sender) =>
            {
                var profileId = httpContext.User.GetProfileId();
                
                var query = new GetImprovementGoalsWithSelectionQuery(profileId, paginationRequest);   
                
                var result = await sender.Send(query);
                
                var response = new GetImprovementGoalsWithSelectionV2Response(result.Goals);
                
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetImprovementGoalsWithSelection v2")
            .WithTags("ImprovementGoals Version2")
            .WithSummary("Get all improvement goals with selection for a profile")
            .WithDescription("Returns a paginated list of all improvement goals with selection status for a specific profile")
            .Produces<GetImprovementGoalsWithSelectionV2Response>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}