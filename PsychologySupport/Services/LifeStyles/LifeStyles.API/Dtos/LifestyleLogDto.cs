using BuildingBlocks.Enums;

namespace LifeStyles.API.Dtos
{
    public record LifestyleLogDto(
    Guid PatientProfileId,
    DateTimeOffset LogDate,
    SleepHoursLevel SleepHours,
    ExerciseFrequency ExerciseFrequency,
    AvailableTimePerDay AvailableTimePerDay
);
}
