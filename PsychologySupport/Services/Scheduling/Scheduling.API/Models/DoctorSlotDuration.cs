using System.Numerics;

namespace Scheduling.API.Models
{
    public class DoctorSlotDuration
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public int SlotDuration { get; set; } // số phút
        public int SlotsPerDay { get; set; }
    }

}
