namespace ChatBox.API.Domains.AIChats.Dtos.Dashboard;

public sealed record ChatCohortSeriesDto(
    DateOnly CohortWeek,                // tuần D0 (bắt đầu thứ Hai)
    int CohortSize,                     // số user D0 trong tuần đó
    IReadOnlyList<ChatCohortPointDto> Points
);