using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using MassTransit;
using Scheduling.API.Enums;

namespace Scheduling.API.Features.UpdateBooking
{

    public record UpdateBookingCommand(string BookingCode, BookingStatus Status) : ICommand<UpdateBookingResult>;

    public record UpdateBookingResult(string BookingCode);

    public class UpdateBookingHandler : ICommandHandler<UpdateBookingCommand, UpdateBookingResult>
    {
        private readonly SchedulingDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public UpdateBookingHandler(SchedulingDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<UpdateBookingResult> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingCode == request.BookingCode, cancellationToken)
                ?? throw new NotFoundException("Booking", request.BookingCode);

            booking.Status = request.Status;
            await _context.SaveChangesAsync(cancellationToken);

            return new UpdateBookingResult(booking.BookingCode);
        }
    }
}
