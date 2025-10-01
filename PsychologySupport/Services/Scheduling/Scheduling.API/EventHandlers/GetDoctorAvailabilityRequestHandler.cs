using BuildingBlocks.Messaging.Events.Queries.Scheduling;
using MassTransit;

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

            var dateOnlyStart = DateOnly.FromDateTime(startDate.DateTime);
            var dateOnlyEnd = DateOnly.FromDateTime(endDate.DateTime);

            var doctorData = await _context.DoctorSlotDurations
                .Where(d => request.DoctorIds.Contains(d.DoctorId))
                .Select(d => new
                {
                    d.DoctorId,
                    d.SlotDuration,
                    UnavailableSlots = _context.DoctorAvailabilities
                        .Where(da => da.DoctorId == d.DoctorId &&
                                     da.Date >= dateOnlyStart && da.Date <= dateOnlyEnd)
                        .Select(da => new { da.Date, da.StartTime })
                        .ToList(),
                    BookedSlots = _context.Bookings
                        .Where(b => b.DoctorId == d.DoctorId &&
                                    b.Date >= dateOnlyStart && b.Date <= dateOnlyEnd)
                        .Select(b => new { b.Date, b.StartTime, b.Duration })
                        .ToList()
                })
                .AsNoTracking()
                .ToListAsync();

            var doctorAvailabilityMap = new Dictionary<Guid, bool>();

            foreach (var doctor in doctorData)
            {
                var unavailableSet = doctor.UnavailableSlots
                    .Select(da => (da.Date, da.StartTime))
                    .ToHashSet();

                var bookedSet = doctor.BookedSlots
                    .Select(b => (b.Date, b.StartTime))
                    .ToHashSet();

                bool isBusy = unavailableSet.Any() || bookedSet.Any();

                doctorAvailabilityMap[doctor.DoctorId] = !isBusy;
            }

            await context.RespondAsync(new GetDoctorAvailabilityResponse(doctorAvailabilityMap));
        }
    }
}
