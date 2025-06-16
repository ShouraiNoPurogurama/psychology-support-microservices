using BuildingBlocks.Enums;

namespace LifeStyles.API.Models
{
    public class LifestyleLog
    {
        public Guid Id { get; set; }

        public Guid PatientProfileId { get; set; }

        public DateTimeOffset LogDate { get; set; }

        public SleepHoursLevel SleepHours { get; set; }

        public ExerciseFrequency ExerciseFrequency { get; set; }

        public AvailableTimePerDay AvailableTimePerDay { get; set; }

    }
}
