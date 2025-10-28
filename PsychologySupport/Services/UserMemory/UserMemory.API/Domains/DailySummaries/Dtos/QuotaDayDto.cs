namespace UserMemory.API.Domains.DailySummaries.Dtos;

public record QuotaDayDto(
    DateOnly Date,
    int QuotaUsed,
    int QuotaTotal,
    int QuotaRemaining,
    bool IsToday,
    bool CanClaim
);
