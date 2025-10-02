namespace Scheduling.API.Models
{
    public class Schedule
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid? DoctorId { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<ScheduleFeedback> Feedbacks { get; set; } = new List<ScheduleFeedback>();
    }
}
