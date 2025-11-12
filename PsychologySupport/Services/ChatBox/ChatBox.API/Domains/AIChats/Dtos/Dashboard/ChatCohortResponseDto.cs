namespace ChatBox.API.Domains.AIChats.Dtos.Dashboard;

public sealed record ChatCohortResponseDto(
    IReadOnlyList<ChatCohortSeriesDto> Series
);