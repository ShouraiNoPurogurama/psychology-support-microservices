using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Dtos;
using LifeStyles.API.Extensions;
using MediatR;

namespace LifeStyles.API.Features.ImprovementGoal.GetImprovementGoalsWithSelection;


public record GetImprovementGoalsWithSelectionResponse(PaginatedResult<ImprovementGoalWithSelectionDto> Goals);

public class GetImprovementGoalsWithSelectionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/me/improvement-goals", 
            async ([AsParameters] PaginationRequest paginationRequest, HttpContext httpContext, ISender sender) =>
            {
                var profileId = httpContext.User.GetProfileId();
                
                var query = new GetImprovementGoalsWithSelectionQuery(profileId, paginationRequest);   
                
                var result = await sender.Send(query);
                
                var response = new GetImprovementGoalsWithSelectionResponse(result.Goals);
                
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetImprovementGoalsWithSelection")
            .WithTags("ImprovementGoals")
            .WithSummary("Get all improvement goals with selection for a profile")
            .WithDescription("Returns a paginated list of all improvement goals with selection status for a specific profile")
            .Produces<GetImprovementGoalsWithSelectionResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}