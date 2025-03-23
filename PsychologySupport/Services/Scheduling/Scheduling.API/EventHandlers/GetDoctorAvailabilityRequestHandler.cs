using BuildingBlocks.Messaging.Events.Scheduling;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Scheduling.API.EventHandlers
{
    public class GetDoctorAvailabilityRequestHandler : IConsumer<GetDoctorAvailabilityRequest>
    {
        private readonly SchedulingDbContext _context;

        public GetDoctorAvailabilityRequestHandler(SchedulingDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<GetDoctorAvailabilityRequest> context)
        {
            var request = context.Message;
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // GMT+7
            var startDate = TimeZoneInfo.ConvertTime(request.StartDate, vietnamTimeZone);
            var endDate = TimeZoneInfo.ConvertTime(request.EndDate, vietnamTimeZone);

            var doctorAvailabilityMap = new Dictionary<Guid, bool>();

            // 🔍 Truy vấn dữ liệu 1 lần thay vì trong vòng lặp
            var doctorSlots = await _context.DoctorSlotDurations
                .Where(d => request.DoctorIds.Contains(d.DoctorId))
                .AsNoTracking()
                .ToListAsync();

            var unavailableSlots = await _context.DoctorAvailabilities
                .Where(da => request.DoctorIds.Contains(da.DoctorId) &&
                             da.Date >= DateOnly.FromDateTime(startDate) &&
                             da.Date <= DateOnly.FromDateTime(endDate))
                .AsNoTracking()
                .ToListAsync();

            var bookedSlots = await _context.Bookings
                .Where(b => request.DoctorIds.Contains(b.DoctorId) &&
                            b.Date >= DateOnly.FromDateTime(startDate) &&
                            b.Date <= DateOnly.FromDateTime(endDate))
                .AsNoTracking()
                .ToListAsync();

            // 🔄 Kiểm tra từng bác sĩ song song
            await Parallel.ForEachAsync(request.DoctorIds, async (doctorId, _) =>
            {
                var doctorSlot = doctorSlots.FirstOrDefault(d => d.DoctorId == doctorId);
                if (doctorSlot == null)
                {
                    doctorAvailabilityMap[doctorId] = false;
                    return;
                }

                int slotDuration = doctorSlot.SlotDuration;

                // 🔍 Lọc danh sách lịch bận của bác sĩ hiện tại
                var doctorBusySlots = unavailableSlots
                    .Where(da => da.DoctorId == doctorId)
                    .Select(da => new
                    {
                        da.Date,
                        StartTime = da.StartTime,
                        EndTime = da.StartTime.AddMinutes(slotDuration)
                    })
                    .ToList();

                var doctorBookedSlots = bookedSlots
                    .Where(b => b.DoctorId == doctorId)
                    .Select(b => new
                    {
                        b.Date,
                        StartTime = b.StartTime,
                        EndTime = b.StartTime.AddMinutes(b.Duration)
                    })
                    .ToList();

                // 📌 Kiểm tra trùng lịch
                bool isBusy = doctorBusySlots.Any(busy =>
                 busy.Date == DateOnly.FromDateTime(startDate) &&
                (startDate.TimeOfDay < busy.EndTime.ToTimeSpan() && endDate.TimeOfDay > busy.StartTime.ToTimeSpan()))
                || doctorBookedSlots.Any(booked =>
                booked.Date == DateOnly.FromDateTime(startDate) &&
                (startDate.TimeOfDay < booked.EndTime.ToTimeSpan() && endDate.TimeOfDay > booked.StartTime.ToTimeSpan()));


                doctorAvailabilityMap[doctorId] = !isBusy;
            });

            await context.RespondAsync(new GetDoctorAvailabilityResponse(doctorAvailabilityMap));
        }
    }
}
