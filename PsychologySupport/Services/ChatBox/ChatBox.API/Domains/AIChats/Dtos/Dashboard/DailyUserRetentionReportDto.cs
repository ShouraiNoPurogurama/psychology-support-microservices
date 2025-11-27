namespace ChatBox.API.Domains.AIChats.Dtos.Dashboard;

public record DailyUserRetentionReportDto(
    IReadOnlyList<DailyUserRetentionPointDto> Points,
    decimal CurrentTotalUsers,       
    decimal AverageRetentionRate);