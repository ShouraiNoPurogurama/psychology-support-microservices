using ChatBox.API.Domains.AIChats.Dtos.Dashboard;
using ChatBox.API.Models.Views;

namespace ChatBox.API.Domains.AIChats.Services.Contracts;

public interface IDashboardService
{
    Task<ChatCohortResponseDto> GetChatCohortsAsync(
        DateOnly startDate,
        int maxWeeks,
        CancellationToken ct = default);

    Task<UserOnscreenStatsDto> GetUsersChatOnscreenStatsAsync(
        DateOnly startDate,
        int maxWeeks = 7,
        CancellationToken ct = default);

    Task<DailyUserRetentionReportDto> GetRetentionReportAsync(
        DateOnly startDate,
        int maxWeeks = 7,
        CancellationToken ct = default);

    Task<List<WeeklyRetentionStat>> GetWeeklyRetentionCurveAsync(int limitWeeks = 12, CancellationToken ct = default);
}