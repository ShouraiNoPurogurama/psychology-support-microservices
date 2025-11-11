using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using Microsoft.EntityFrameworkCore;
using UserMemory.API.Data;
using UserMemory.API.Shared.Authentication;
using UserMemory.API.Shared.Services.Contracts;
using UserMemory.API.Data.Options;

namespace UserMemory.API.Domains.DailySummaries.Features.GetMyDailyRewardQuotas;

public record GetMyDailyRewardQuotasQuery : IQuery<GetMyDailyRewardQuotasResult>;

public record GetMyDailyRewardQuotasResult(
    SubscriptionTier Tier,
    bool CanClaimNow,
    string? Reason,
    DateTimeOffset Now,
    string Timezone,
    int TodayTotal,
    int TodayUsed,
    int TodayRemaining
);

public class GetMyQuotaNowHandler(
    UserMemoryDbContext db,
    ICurrentActorAccessor actor,
    ICurrentUserSubscriptionAccessor subscriptionAccessor
) : IQueryHandler<GetMyDailyRewardQuotasQuery, GetMyDailyRewardQuotasResult>
{
    public async Task<GetMyDailyRewardQuotasResult> Handle(GetMyDailyRewardQuotasQuery request, CancellationToken ct)
    {
        var tz = TimeSpan.FromHours(7);
        var now = DateTimeOffset.UtcNow.ToOffset(tz);
        var today = DateOnly.FromDateTime(now.Date);
        var aliasId = actor.GetRequiredAliasId();
        var userTier = subscriptionAccessor.GetCurrentTier();
        var dailyLimit = StickerGenerationQuotaOptions.GetMaxClaimsForTier(userTier);
        var todayUsed = await db.AliasDailySummaries
            .Where(x => x.AliasId == aliasId && x.Date == today)
            .SumAsync(x => (int?)x.RewardClaimCount, ct) ?? 0;
        var todayRemaining = Math.Max(0, dailyLimit - todayUsed);
        var canClaim = todayRemaining > 0;
        var reason = canClaim ? null : "DAILY_LIMIT_REACHED";
        return new GetMyDailyRewardQuotasResult(
            Tier: userTier,
            CanClaimNow: canClaim,
            Reason: reason,
            Now: now,
            Timezone: "Asia/Ho_Chi_Minh",
            TodayTotal: dailyLimit,
            TodayUsed: todayUsed,
            TodayRemaining: todayRemaining
        );
    }
}