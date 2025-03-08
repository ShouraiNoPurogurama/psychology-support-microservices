namespace Scheduling.API.Models
{
    public class ScheduleFeedback
    {
        public Guid Id { get; set; }
        public Guid ScheduleId { get; set; }
        public Guid PatientId { get; set; }
        public string? Content { get; set; }
        public int Rating { get; set; }
        public DateTime FeedbackDate { get; set; }
    }
}
