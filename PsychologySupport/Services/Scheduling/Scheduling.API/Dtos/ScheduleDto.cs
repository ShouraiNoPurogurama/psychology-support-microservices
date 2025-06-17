namespace Scheduling.API.Dtos
{
    public class ScheduleDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid? DoctorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int TotalActivityCount { get; set; }

        public List<SessionDto> Sessions { get; set; } = new List<SessionDto>();
    }
}