using BuildingBlocks.Enums;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserMemory.API.Domains.DailySummaries.Dtos;

namespace UserMemory.API.Domains.DailySummaries.Features.GetMyDailyRewardQuotas;

public record GetMyDailyRewardQuotasResponse(
    SubscriptionTier Tier,
    bool CanClaimNow,
    string? Reason,
    DateTimeOffset Now,
    string Timezone,
    int TodayTotal,
    int TodayUsed,
    int TodayRemaining
);

public class GetMyDailyRewardQuotasEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("v1/me/quotas", async (
                ISender sender) =>
            {
                var query = new GetMyDailyRewardQuotasQuery();
                
                var result = await sender.Send(query);

                var response = result.Adapt<GetMyDailyRewardQuotasResponse>();
                
                return Results.Ok(response);
            })
            .WithTags("Quotas")
            .WithName("GetMyDailyRewardQuotas")
            .Produces<GetMyDailyRewardQuotasResult>(StatusCodes.Status200OK)
            .WithSummary("Get My Daily Reward Quotas")
            .WithDescription("Quota claim reward theo ngày trong một khoảng thời gian, kèm tổng hợp và trạng thái hôm nay.");
    }
}