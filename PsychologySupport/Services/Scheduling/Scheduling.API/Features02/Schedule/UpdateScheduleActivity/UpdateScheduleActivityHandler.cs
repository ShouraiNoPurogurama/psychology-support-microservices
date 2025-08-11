using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using MassTransit;
using Scheduling.API.Enums;

namespace Scheduling.API.Features02.Schedule.UpdateScheduleActivity
{
    public record UpdateScheduleActivityCommand(Guid Id, Guid SessionId, ScheduleActivityStatus Status) : ICommand<UpdateScheduleActivityResult>;

    public record UpdateScheduleActivityResult(Guid Id, Guid SessionId);

    public class UpdateScheduleActivityHandler : ICommandHandler<UpdateScheduleActivityCommand, UpdateScheduleActivityResult>
    {
        private readonly SchedulingDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public UpdateScheduleActivityHandler(SchedulingDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<UpdateScheduleActivityResult> Handle(UpdateScheduleActivityCommand request, CancellationToken cancellationToken)
        {
            var activity = await _context.ScheduleActivities
                 .FirstOrDefaultAsync(a =>
                     (a.EntertainmentActivityId == request.Id ||
                      a.FoodActivityId == request.Id ||
                      a.PhysicalActivityId == request.Id ||
                      a.TherapeuticActivityId == request.Id)
                     && a.SessionId == request.SessionId, cancellationToken);


            if (activity == null)
                throw new NotFoundException("ScheduleActivity", request.Id);

            activity.Status = request.Status;
            await _context.SaveChangesAsync(cancellationToken);

            return new UpdateScheduleActivityResult(activity.Id, activity.SessionId);
        }
    }
}
