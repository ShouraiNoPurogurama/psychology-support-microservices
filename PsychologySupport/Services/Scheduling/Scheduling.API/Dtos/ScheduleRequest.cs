namespace Scheduling.API.Dtos
{
    public class ScheduleRequest
    {
        public Guid PatientId { get; set; }
        public int Depression { get; set; }
        public int Anxiety { get; set; }
        public int Stress { get; set; }
    }
}
