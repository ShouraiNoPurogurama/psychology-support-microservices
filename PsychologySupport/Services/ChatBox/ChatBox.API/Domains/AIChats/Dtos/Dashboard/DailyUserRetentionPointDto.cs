namespace ChatBox.API.Domains.AIChats.Dtos.Dashboard;

public record DailyUserRetentionPointDto(
    DateTime ActivityDate,
    long TotalActiveUsers,
    long NewUsers,
    long ReturningUsers,
    decimal TotalUsersToDate,
    decimal ReturningPercentage);
