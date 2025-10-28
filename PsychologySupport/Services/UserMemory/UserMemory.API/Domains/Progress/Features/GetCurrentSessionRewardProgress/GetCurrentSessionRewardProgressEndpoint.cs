using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UserMemory.API.Domains.Progress.Features.GetCurrentSessionRewardProgress;

public record GetCurrentSessionRewardProgressResponse(
    Guid SessionId,
    int ProgressPoint,
    int LastIncrement,
    DateOnly ProgressDate,
    int ClaimedToday,    // số lượt đã claim hôm nay
    int DailyLimit,      // limit cứng (vd 3)
    int RemainingToday,  // = max(0, DailyLimit - ClaimedToday)
    bool CanClaim        // RemainingToday > 0 && ProgressPoint >= threshold
);
public class GetCurrentSessionRewardProgressEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/progress/{sessionId}/reward", async ([FromRoute] Guid sessionId,ISender sender) =>
        {
            var query = new GetCurrentSessionRewardProgressQuery(sessionId);
            
            var result = await sender.Send(query);
            
            var response = result.Adapt<GetCurrentSessionRewardProgressResponse>();
            
            return Results.Ok(response);
        })
            .WithTags("Progress")
            .WithName("GetCurrentSessionRewardProgress")
            .Produces<GetCurrentSessionRewardProgressResponse>(StatusCodes.Status200OK)
            .WithDescription("Gets the current reward progress for a session.")
            .WithSummary("Get Current Session Reward Progress");
            
    }
}