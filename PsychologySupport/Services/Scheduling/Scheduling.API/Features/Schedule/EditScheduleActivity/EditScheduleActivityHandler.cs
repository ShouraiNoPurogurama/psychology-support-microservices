using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.Schedule.EditScheduleActivity
{
    public record EditScheduleActivityCommand(Guid Id, Guid SessionId, EditScheduleActivityDto EditDto)
        : ICommand<EditScheduleActivityResult>;

    public record EditScheduleActivityResult(Guid Id, Guid SessionId);

    public class EditScheduleActivityHandler : ICommandHandler<EditScheduleActivityCommand, EditScheduleActivityResult>
    {
        private readonly SchedulingDbContext _context;

        public EditScheduleActivityHandler(SchedulingDbContext context)
        {
            _context = context;
        }

        public async Task<EditScheduleActivityResult> Handle(EditScheduleActivityCommand request,
            CancellationToken cancellationToken)
        {
            var activity = await _context.ScheduleActivities
                .FirstOrDefaultAsync(a => (a.EntertainmentActivityId == request.Id ||
                                           a.FoodActivityId == request.Id ||
                                           a.PhysicalActivityId == request.Id ||
                                           a.TherapeuticActivityId == request.Id)
                                          && a.SessionId == request.SessionId && a.SessionId == request.SessionId,
                    cancellationToken);

            if (activity == null)
                throw new NotFoundException("ScheduleActivity", request.Id);

            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

            if (session == null)
                throw new NotFoundException("Session", request.SessionId);

            /* var vietnamTime = DateTime.UtcNow.AddHours(7); // UTC VietNam
             if (vietnamTime.AddDays(2) > activity.TimeRange || vietnamTime.AddDays(2) > session.StartDate)
                 throw new ValidationException("The scheduled time and session start date must be at least 2 days after the current time.");
 */
            activity.EntertainmentActivityId = request.EditDto.EntertainmentActivityId;
            activity.FoodActivityId = request.EditDto.FoodActivityId;
            activity.PhysicalActivityId = request.EditDto.PhysicalActivityId;
            activity.TherapeuticActivityId = request.EditDto.TherapeuticActivityId;
            activity.Description = request.EditDto.Description;
            activity.Duration = request.EditDto.Duration;

            await _context.SaveChangesAsync(cancellationToken);

            return new EditScheduleActivityResult(activity.Id, activity.SessionId);
        }
    }
}