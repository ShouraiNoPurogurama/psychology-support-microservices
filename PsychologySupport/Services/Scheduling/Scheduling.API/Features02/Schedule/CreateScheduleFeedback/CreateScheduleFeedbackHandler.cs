using BuildingBlocks.CQRS;
using Scheduling.API.Exceptions;
using Scheduling.API.Models;

namespace Scheduling.API.Features02.Schedule.CreateScheduleFeedback
{
    public record CreateScheduleFeedbackCommand(
        Guid ScheduleId,
        Guid PatientId,
        string? Content,
        int Rating,
        DateTime FeedbackDate
    ) : ICommand<CreateScheduleFeedbackResult>;
    public record CreateScheduleFeedbackResult(bool IsSucceeded);
    public class CreateScheduleFeedbackHandler : ICommandHandler<CreateScheduleFeedbackCommand, CreateScheduleFeedbackResult>
    {
        private readonly SchedulingDbContext _context;

        public CreateScheduleFeedbackHandler(SchedulingDbContext context)
        {
            _context = context;
        }

        public async Task<CreateScheduleFeedbackResult> Handle(CreateScheduleFeedbackCommand request, CancellationToken cancellationToken)
        {
            var schedule = await _context.Schedules.FindAsync([request.ScheduleId], cancellationToken: cancellationToken);
            if (schedule == null)
            {
                throw new ScheduleNotFoundException(request.ScheduleId);
            }

            var feedback = new ScheduleFeedback
            {
                Id = Guid.NewGuid(),
                ScheduleId = request.ScheduleId,
                PatientId = request.PatientId,
                Content = request.Content,
                Rating = request.Rating,
                FeedbackDate = request.FeedbackDate
            };

            _context.ScheduleFeedbacks.Add(feedback);
            await _context.SaveChangesAsync(cancellationToken);

            return new CreateScheduleFeedbackResult(true);
        }
    }
}
