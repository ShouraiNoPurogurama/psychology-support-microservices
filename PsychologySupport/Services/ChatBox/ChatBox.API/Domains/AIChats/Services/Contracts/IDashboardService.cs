using ChatBox.API.Domains.AIChats.Dtos.Dashboard;

namespace ChatBox.API.Domains.AIChats.Services.Contracts;

public interface IDashboardService
{
    Task<ChatCohortResponseDto> GetChatCohortsAsync(
        DateOnly startDate,
        int maxWeeks,
        CancellationToken ct = default);
}