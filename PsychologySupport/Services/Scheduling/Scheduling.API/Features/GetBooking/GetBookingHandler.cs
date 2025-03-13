using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Mapster;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.GetBooking
{
    public record GetBookingQuery(string BookingCode) : IQuery<GetBookingResult>;

    public record GetBookingResult(BookingDto Booking);
    public class GetBookingHandler(SchedulingDbContext context) : IQueryHandler<GetBookingQuery, GetBookingResult>
    {
        public async Task<GetBookingResult> Handle(GetBookingQuery request, CancellationToken cancellationToken)
        {
            var booking = await context.Bookings
                .FirstOrDefaultAsync(b => b.BookingCode == request.BookingCode, cancellationToken)
                ?? throw new NotFoundException("Booking", request.BookingCode);

            var bookingDto = booking.Adapt<BookingDto>();
            return new GetBookingResult(bookingDto);
        }
    }
}
