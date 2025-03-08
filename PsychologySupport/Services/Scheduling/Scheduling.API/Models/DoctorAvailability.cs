using System.Numerics;

namespace Scheduling.API.Models
{
    public class DoctorAvailability
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public DateOnly Date { get; set; }  
        public TimeOnly StartTime { get; set; }  // Slot bị bận
    }

}
