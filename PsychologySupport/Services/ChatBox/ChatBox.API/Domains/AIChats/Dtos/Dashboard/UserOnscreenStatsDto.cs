namespace ChatBox.API.Domains.AIChats.Dtos.Dashboard;

public record UserOnscreenStatsDto(
    IReadOnlyList<UserOnscreenPointDto> Points,
    long TotalActiveUsersAllTime,
    double TotalSystemOnscreenSecondsAllTime,
    double AvgOnscreenSecondsPerUserAllTime);