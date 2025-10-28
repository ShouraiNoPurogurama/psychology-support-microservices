namespace UserMemory.API.Domains.DailySummaries.Dtos;

public record GetMyDailyRewardQuotasSummary(
    int TotalDays,
    int TotalUsed,
    int TotalRemaining
);