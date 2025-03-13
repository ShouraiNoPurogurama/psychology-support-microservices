using Scheduling.API.Enums;

namespace Scheduling.API.Dtos
{
    public struct TimeSlotDto
    {
        public SlotStatus Status { get; set; }
        public string DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string OccupiedInfo { get; set; } 

        public TimeSlotDto(SlotStatus status, string dayOfWeek, TimeOnly startTime, TimeOnly endTime, string occupiedInfo = "")
        {
            Status = status;
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
            OccupiedInfo = occupiedInfo;
        }
    }
}
