namespace UserMemory.API.Domains.DailySummaries.Dtos;

public record GetMyDailyRewardQuotasSummary(
    int TotalDays,
    int TotalUsed,
    int TotalRemaining,
    int ThisDayUsed,
    int? ThisWeekUsed,
    int? ThisMonthUsed,
    int? ThisDayRemaining,
    int? ThisWeekRemaining,
    int? ThisMonthRemaining,
    DateOnly PeriodStart,
    DateOnly PeriodEnd
);