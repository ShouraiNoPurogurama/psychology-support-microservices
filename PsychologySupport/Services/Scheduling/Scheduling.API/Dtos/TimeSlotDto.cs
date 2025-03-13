using Scheduling.API.Enums;

namespace Scheduling.API.Dtos
{
    public struct  TimeSlotDto
    {
        public SlotStatus Status { get; set; }
        public string DayOfWeek { get; }
        public TimeOnly StartTime { get; }
        public TimeOnly EndTime { get; }


        public TimeSlotDto(SlotStatus status, string dayOfWeek, TimeOnly startTime, TimeOnly endTime)
        {
            Status = status;
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
        }
    }


}
