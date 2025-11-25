namespace Profile.API.Domains.Pii.Dtos;

public sealed record DailyNewUserStatsDto(
    IReadOnlyList<DailyNewUserPointDto> Points
);