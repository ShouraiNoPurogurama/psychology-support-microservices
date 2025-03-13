using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Scheduling.API.Dtos;
using Scheduling.API.Enums;

namespace Scheduling.API.Features.GetDoctorSchedule
{
    public record GetDoctorScheduleQuery(Guid DoctorId, DateOnly Date) : IQuery<GetDoctorScheduleResult>;
    public record GetDoctorScheduleResult(List<TimeSlotDto> TimeSlots);

    public class GetDoctorScheduleHandler(SchedulingDbContext context)
        : IQueryHandler<GetDoctorScheduleQuery, GetDoctorScheduleResult>
    {
        public async Task<GetDoctorScheduleResult> Handle(GetDoctorScheduleQuery request, CancellationToken cancellationToken)
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // GMT+7
            var requestDateTime = request.Date.ToDateTime(TimeOnly.MinValue);
            var vietnamDateTime = TimeZoneInfo.ConvertTime(requestDateTime, vietnamTimeZone);
            var vietnamDate = DateOnly.FromDateTime(vietnamDateTime); 

            var doctorSlots = await context.DoctorSlotDurations
                .FirstOrDefaultAsync(d => d.DoctorId == request.DoctorId, cancellationToken)
                ?? throw new NotFoundException("Doctor Slot Duration", request.DoctorId);

            var bookedSlots = await context.Bookings
                .Where(b => b.DoctorId == request.DoctorId && b.Date == vietnamDate)
                .Select(b => new { b.BookingCode,b.StartTime, b.Duration })
                .ToListAsync(cancellationToken);

            var unavailableSlots = await context.DoctorAvailabilities
                .Where(da => da.DoctorId == request.DoctorId && da.Date == vietnamDate)
                .Select(da => da.StartTime)
                .ToListAsync(cancellationToken);

            var timeSlots = new List<TimeSlotDto>();

            var dayOfWeek = vietnamDate.DayOfWeek;

            var dayTemplates = await context.TimeSlotTemplates
                .Where(t => t.DayOfWeek == dayOfWeek)
                .ToListAsync(cancellationToken);

            
            timeSlots = dayTemplates
                .SelectMany(template => GenerateTimeSlots(request.Date, template.StartTime, doctorSlots.SlotDuration, template.EndTime))
                .Select(slot =>
                {
                    var bookedSlot = bookedSlots.FirstOrDefault(b => b.StartTime <= slot.StartTime &&
                                                                     b.StartTime.Add(TimeSpan.FromMinutes(b.Duration)) > slot.StartTime);
                    bool isUnavailable = bookedSlot != null || unavailableSlots.Contains(slot.StartTime);
                    string occupiedInfo = bookedSlot != null ? $"BookingCode: {bookedSlot.BookingCode}"
                                        : (unavailableSlots.Contains(slot.StartTime) ? "Doctor unavailable" : "");

                    return new TimeSlotDto(
                        isUnavailable ? SlotStatus.Unavailable : SlotStatus.Available,
                        slot.DayOfWeek,
                        slot.StartTime,
                        slot.EndTime,
                        occupiedInfo
                    );
                })
                .ToList();

            return new GetDoctorScheduleResult(timeSlots);
        }


        private List<TimeSlotDto> GenerateTimeSlots(DateOnly date, TimeOnly startTime, int slotDuration, TimeOnly endTime)
        {
            var slots = new List<TimeSlotDto>();
            while (startTime < endTime)
            {
                var endSlotTime = startTime.Add(TimeSpan.FromMinutes(slotDuration));
                if (endSlotTime > endTime) break;
                slots.Add(new TimeSlotDto(SlotStatus.Available, date.DayOfWeek.ToString(), startTime, endSlotTime));
                startTime = endSlotTime;
            }
            return slots;
        }
    }
}
