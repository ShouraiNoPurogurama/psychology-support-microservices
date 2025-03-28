namespace Scheduling.API.Models
{
    public class TimeSlotTemplate
    {
        public Guid Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; } // (Monday, Tuesday,...)
        public TimeOnly StartTime { get; set; } // (08:00)
        public TimeOnly EndTime { get; set; } // (17:00)
    }
}
