using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using UserMemory.API.Data;
using UserMemory.API.Data.Options;
using UserMemory.API.Domains.DailySummaries.Dtos;
using UserMemory.API.Shared.Authentication;

namespace UserMemory.API.Domains.DailySummaries.Features.GetMyDailyRewardQuotas;

public record GetMyDailyRewardQuotasQuery(DateOnly? StartDate, DateOnly? EndDate) : IQuery<GetMyDailyRewardQuotasResult>;

public record GetMyDailyRewardQuotasResult(
    int DailyLimit,
    DateOnly StartDate,
    DateOnly EndDate,
    QuotaDayDto Today,
    IReadOnlyList<QuotaDayDto> Days,
    GetMyDailyRewardQuotasSummary Summary
);

public class GetMyDailyRewardQuotasHandler(
    UserMemoryDbContext dbContext,
    ICurrentActorAccessor currentActorAccessor)
    : IQueryHandler<GetMyDailyRewardQuotasQuery, GetMyDailyRewardQuotasResult>
{
    public async Task<GetMyDailyRewardQuotasResult> Handle(
        GetMyDailyRewardQuotasQuery request, CancellationToken ct)
    {
        int maxFreeClaimsPerDay = QuotaOptions.MAX_FREE_CLAIMS_PER_DAY;


        var aliasId = currentActorAccessor.GetRequiredAliasId();

        // NOTE: nếu có timezone user thì convert về local trước khi DateOnly.
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        // Mặc định 7 ngày gần nhất (bao gồm hôm nay)
        var start = request.StartDate ?? today.AddDays(-6);
        var end = request.EndDate ?? today;
        if (end < start) (start, end) = (end, start);

        // Lấy dữ liệu đã có trong bảng (left join thủ công để lấp đầy ngày trống)
        var rows = await dbContext.AliasDailySummaries
            .Where(x => x.AliasId == aliasId && x.Date >= start && x.Date <= end)
            .Select(x => new { x.Date, x.RewardClaimCount })
            .ToListAsync(ct);

        var days = new List<QuotaDayDto>();
        for (var d = start; d <= end; d = d.AddDays(1))
        {
            var used = rows.FirstOrDefault(r => r.Date == d)?.RewardClaimCount ?? 0;
            var remaining = Math.Max(0, maxFreeClaimsPerDay - used);

            days.Add(new QuotaDayDto(
                Date: d,
                QuotaUsed: used,
                QuotaTotal: maxFreeClaimsPerDay,
                QuotaRemaining: remaining,
                IsToday: d == today,
                CanClaim: d == today && remaining > 0
            ));
        }

        var todayDto = days.FirstOrDefault(x => x.IsToday)
                       ?? new QuotaDayDto(today, 0, maxFreeClaimsPerDay, maxFreeClaimsPerDay, true, maxFreeClaimsPerDay > 0);

        var summary = new GetMyDailyRewardQuotasSummary(
            TotalDays: days.Count,
            TotalUsed: days.Sum(x => x.QuotaUsed),
            TotalRemaining: days.Sum(x => x.QuotaRemaining)
        );

        return new GetMyDailyRewardQuotasResult(
            DailyLimit: maxFreeClaimsPerDay,
            StartDate: start,
            EndDate: end,
            Today: todayDto,
            Days: days.OrderBy(x => x.Date).ToList(),
            Summary: summary
        );
    }
}