namespace Scheduling.API.Dtos
{
    public class SessionDto
    {
        public Guid ScheduleId { get; set; }
        public string Purpose { get; set; }
        public int Order { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
