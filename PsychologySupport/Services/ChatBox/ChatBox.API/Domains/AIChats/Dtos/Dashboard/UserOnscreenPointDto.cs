namespace ChatBox.API.Domains.AIChats.Dtos.Dashboard;

public record UserOnscreenPointDto(
    DateTime ActivityDate,
    long TotalActiveUsers,
    double TotalSystemOnscreenSeconds,
    double AvgOnscreenSecondsPerUser);