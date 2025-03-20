using BuildingBlocks.CQRS;
using Microsoft.AspNetCore.Mvc;

namespace Scheduling.API.Features.GetTotalBooking
{
    public record GetTotalBookingQuery(
        [FromQuery] DateOnly StartDate,
        [FromQuery] DateOnly EndDate,
        [FromQuery] Guid? DoctorId = null
    ) : IQuery<GetTotalBookingResult>;

    public record GetTotalBookingResult(int TotalBookings);

    public class GetTotalBookingHandler : IQueryHandler<GetTotalBookingQuery, GetTotalBookingResult>
    {
        private readonly SchedulingDbContext _context;

        public GetTotalBookingHandler(SchedulingDbContext context)
        {
            _context = context;
        }

        public async Task<GetTotalBookingResult> Handle(
            GetTotalBookingQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Bookings.AsQueryable();

           
            query = query.Where(booking =>
                booking.Date >= request.StartDate && booking.Date <= request.EndDate);

   
            if (request.DoctorId.HasValue)
            {
                query = query.Where(booking => booking.DoctorId == request.DoctorId);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            return new GetTotalBookingResult(totalCount);
        }
    }
}
