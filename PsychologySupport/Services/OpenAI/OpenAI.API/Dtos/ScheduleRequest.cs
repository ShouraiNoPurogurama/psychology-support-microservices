namespace OpenAI.API.Dtos
{
    public class ScheduleRequest
    {
        public string ScheduleId { get; set; }
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public Session[] Sessions { get; set; }
    }

    public class Session
    {
        public string Id { get; set; }
        public string Purpose { get; set; }
        public int Order { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public Activity[] Activities { get; set; }
    }

    public class Activity
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string TimeRange { get; set; }
        public string Duration { get; set; }
        public int DateNumber { get; set; }
        public string Status { get; set; }
    }

}
