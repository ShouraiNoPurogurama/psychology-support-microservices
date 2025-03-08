using BuildingBlocks.Dtos;

namespace Scheduling.API.Dtos
{
    public class ScheduleActivityDto
    {
        public Guid SessionId { get; set; }
        public EntertainmentActivityDto? EntertainmentActivity { get; set; }
        public FoodActivityDto? FoodActivity { get; set; }
        public PhysicalActivityDto? PhysicalActivity { get; set; }
        public TherapeuticActivityDto? TherapeuticActivity { get; set; }
        public string Description { get; set; }
        public DateTime TimeRange { get; set; }
        public string Duration { get; set; }
        public int DateNumber { get; set; }
        public string Status { get; set; }
    }
}
