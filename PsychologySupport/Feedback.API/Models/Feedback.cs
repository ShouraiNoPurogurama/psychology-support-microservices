namespace Feedback.API.Models
{
    public class Feedback
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ServiceId { get; set; }

        public string? FeedbackText { get; set; } 

        public int Rating { get; set; }

        public DateTimeOffset CreatedAt { get; set; } 

    }
}
