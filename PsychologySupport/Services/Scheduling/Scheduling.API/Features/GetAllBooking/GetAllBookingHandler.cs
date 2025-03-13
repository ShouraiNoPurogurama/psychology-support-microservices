using BuildingBlocks.CQRS;
using BuildingBlocks.Pagination;
using Microsoft.AspNetCore.Mvc;
using Scheduling.API.Dtos;
using Scheduling.API.Enums;

namespace Scheduling.API.Features.GetAllBooking
{
    public record GetAllBookingsQuery(
        [FromQuery] int PageIndex,
        [FromQuery] int PageSize,
        [FromQuery] string? Search = "", // doctorid, patientid, bookingcode
        [FromQuery] string? SortBy = "date", // sort date or status
        [FromQuery] string? SortOrder = "asc", // asc or desc
        [FromQuery] Guid? DoctorId = null, // filter
        [FromQuery] Guid? PatientId = null, // filter
        [FromQuery] BookingStatus? Status = null) : IQuery<GetAllBookingsResult>; // filter Pending,Confirmed, Completed, Cancelled
                                                                          
    public record GetAllBookingsResult(PaginatedResult<BookingDto> Bookings);

    public class GetAllBookingHandler : IQueryHandler<GetAllBookingsQuery, GetAllBookingsResult>
    {
        private readonly SchedulingDbContext _context;

        public GetAllBookingHandler(SchedulingDbContext context)
        {
            _context = context;
        }

        public async Task<GetAllBookingsResult> Handle(
            GetAllBookingsQuery request, CancellationToken cancellationToken)
        {
            var pageSize = request.PageSize;
            var pageIndex = Math.Max(0, request.PageIndex - 1);

            var query = _context.Bookings.AsQueryable();

            // Search 
            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(booking =>
                    (booking.PatientId.ToString() == request.Search ||
                     booking.DoctorId.ToString() == request.Search ||
                     booking.BookingCode.Contains(request.Search))
                );
            }

            // Filtering

            if (request.DoctorId.HasValue)
                query = query.Where(booking => booking.DoctorId == request.DoctorId);

            if (request.PatientId.HasValue)
                query = query.Where(booking => booking.PatientId == request.PatientId);

            if (request.Status.HasValue)
                query = query.Where(booking => booking.Status == request.Status);

            // Sorting 
            if (request.SortBy == "date")
            {
                query = request.SortOrder == "asc"
                    ? query.OrderBy(booking => booking.Date)
                    : query.OrderByDescending(booking => booking.Date);
            }
            else if (request.SortBy == "status")
            {
                query = request.SortOrder == "asc"
                    ? query.OrderBy(booking => booking.Status)
                    : query.OrderByDescending(booking => booking.Status);
            }

            // Pagination
            var totalCount = await query.CountAsync(cancellationToken);

            var bookings = await query
                 .Skip(pageIndex * pageSize)
                 .Take(pageSize)
                 .ToListAsync(cancellationToken);

            var paginatedResult = new PaginatedResult<BookingDto>(
                pageIndex + 1,
                pageSize,
                totalCount,
                bookings.Select(booking => new BookingDto(
                    booking.BookingCode,
                    booking.DoctorId,
                    booking.PatientId,
                    booking.Date,
                    booking.StartTime,
                    booking.Duration,
                    booking.Price,
                    booking.PromoCodeId,
                    booking.GiftCodeId,
                    booking.Status
                )).ToList()
            );

            return new GetAllBookingsResult(paginatedResult);
        }
    }
}
