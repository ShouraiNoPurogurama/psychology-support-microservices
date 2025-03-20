using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.Profile;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.GetTop5Booking
{
    public record GetTopDoctorsQuery(DateOnly StartDate, DateOnly EndDate) : IQuery<GetTopDoctorsResult>;

    public record GetTopDoctorsResult(List<DoctorBookingDto> TopDoctors);


    public class GetTop5BookingHandler : IQueryHandler<GetTopDoctorsQuery, GetTopDoctorsResult>
    {
        private readonly SchedulingDbContext _context;
        private readonly IRequestClient<GetDoctorProfileRequest> _doctorClient;

        public GetTop5BookingHandler(SchedulingDbContext context, IRequestClient<GetDoctorProfileRequest> doctorClient)
        {
            _context = context;
            _doctorClient = doctorClient;
        }

        public async Task<GetTopDoctorsResult> Handle(
            GetTopDoctorsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Bookings
                .Where(b => b.Date >= request.StartDate && b.Date <= request.EndDate);

            var doctorBookings = await query
                .GroupBy(b => b.DoctorId)
                .Select(g => new { DoctorId = g.Key, TotalBookings = g.Count() })
                .OrderByDescending(d => d.TotalBookings)
                .Take(5)
                .ToListAsync(cancellationToken);

            var topDoctors = new List<DoctorBookingDto>();

            foreach (var dto in doctorBookings)
            {
                var doctorResponse = await _doctorClient.GetResponse<GetDoctorProfileResponse>(
                    new GetDoctorProfileRequest(dto.DoctorId), cancellationToken);

                var doctor = doctorResponse.Message;

                topDoctors.Add(new DoctorBookingDto(
                    dto.DoctorId,
                    doctor.FullName,
                    dto.TotalBookings
                ));
            }

            return new GetTopDoctorsResult(topDoctors);
        }
    }
}
