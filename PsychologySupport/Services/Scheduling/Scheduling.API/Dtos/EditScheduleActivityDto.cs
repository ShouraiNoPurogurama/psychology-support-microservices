using Scheduling.API.Enums;

namespace Scheduling.API.Dtos
{
    public class EditScheduleActivityDto
    {
        public Guid? EntertainmentActivityId { get; set; }
        public Guid? FoodActivityId { get; set; }
        public Guid? PhysicalActivityId { get; set; }
        public Guid? TherapeuticActivityId { get; set; }
        public string Description { get; set; }
        public string Duration { get; set; } // khoảng thời gian thực hiện minutes
    }
}
