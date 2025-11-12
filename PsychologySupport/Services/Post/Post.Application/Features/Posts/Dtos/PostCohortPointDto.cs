namespace Post.Application.Features.Posts.Dtos;

public sealed record PostCohortPointDto(
    DateOnly CohortWeek,   // tuần D0 (bắt đầu thứ Hai)
    int WeekOffset,        // 0 = Week0, 1 = Week1, ...
    int CohortSize,
    int ActiveCount,
    double ActivePercent
);