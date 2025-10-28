using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserMemory.API.Domains.DailySummaries.Dtos;

namespace UserMemory.API.Domains.DailySummaries.Features.GetMyDailyRewardQuotas;

public record GetMyDailyRewardQuotasResponse(
    int DailyLimit,
    DateOnly StartDate,
    DateOnly EndDate,
    QuotaDayDto Today,
    IReadOnlyList<QuotaDayDto> Days,
    GetMyDailyRewardQuotasSummary Summary
);

public class GetMyDailyRewardQuotasEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("v1/me/quotas", async (
                [FromQuery] DateOnly? startDate,
                [FromQuery] DateOnly? endDate,
                ISender sender) =>
            {
                var query = new GetMyDailyRewardQuotasQuery(startDate, endDate);
                
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