namespace Scheduling.API.Dtos
{
    public class SessionDto
    {
        public Guid Id { get; set; }
        public Guid ScheduleId { get; set; }
        public string Purpose { get; set; }
        public int Order { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<ScheduleActivityDto> Activities { get; set; } = new List<ScheduleActivityDto>();
    }
}
