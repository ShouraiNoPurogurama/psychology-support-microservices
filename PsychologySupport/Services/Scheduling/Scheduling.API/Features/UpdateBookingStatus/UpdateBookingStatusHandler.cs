using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Scheduling.API.Enums;

namespace Scheduling.API.Features.UpdateBookingStatus;

public record UpdateBookingStatusCommand(Guid BookingId, BookingStatus Status) : ICommand<UpdateBookingStatusResult>;

public record UpdateBookingStatusResult(bool IsSuccess);

public class UpdateBookingStatusHandler(SchedulingDbContext dbContext)
    : ICommandHandler<UpdateBookingStatusCommand, UpdateBookingStatusResult>
{
    public async Task<UpdateBookingStatusResult> Handle(UpdateBookingStatusCommand request, CancellationToken cancellationToken)
    {
        var booking = await dbContext.Bookings
            .FindAsync([request.BookingId], cancellationToken: cancellationToken);

        if (booking is null)
        {
            throw new NotFoundException($"Không tìm thấy booking với ID {request.BookingId}.");
        }

        booking.Status = request.Status;

        dbContext.Bookings.Update(booking);

        return new UpdateBookingStatusResult(await dbContext.SaveChangesAsync(cancellationToken) > 0);
    }
}