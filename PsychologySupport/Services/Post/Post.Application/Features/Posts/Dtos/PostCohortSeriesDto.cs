namespace Post.Application.Features.Posts.Dtos;

public sealed record PostCohortSeriesDto(
    DateOnly CohortWeek,
    int CohortSize,
    IReadOnlyList<CohortPointDto> Points
);