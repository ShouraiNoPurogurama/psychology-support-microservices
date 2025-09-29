using MediatR;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Domain.Aggregates.Challenges;
using Wellness.Domain.Aggregates.ProcessHistories;
using Wellness.Domain.Enums;
using static Wellness.Domain.Events.ChallengeDomainEvents;

namespace Wellness.Application.Features.Challenges.EventHandler.Domains
{
    public class ChallengeProgressUpdatedEventHandler
         : INotificationHandler<ChallengeProgressUpdatedEvent>
    {
        private readonly IWellnessDbContext _dbContext;

        public ChallengeProgressUpdatedEventHandler(IWellnessDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(ChallengeProgressUpdatedEvent notification, CancellationToken cancellationToken)
        {
            var stepProgress = await _dbContext.ChallengeStepProgresses
                .FirstOrDefaultAsync(x => x.ChallengeStepId == notification.ChallengeStepId
                                        && x.ChallengeProgressId == notification.ChallengeProgressId,
                                     cancellationToken);

            if (stepProgress is null)
            {
                stepProgress = ChallengeStepProgress.Create(notification.ChallengeProgressId, notification.ChallengeStepId);
                _dbContext.ChallengeStepProgresses.Add(stepProgress);
            }

            switch (notification.Status)
            {
                case ProcessStatus.Progressing:
                    stepProgress.Start();
                    break;

                case ProcessStatus.Completed:
                    stepProgress.Complete(notification.PostMoodId);

                    // Tạo ProcessHistory mới
                    var completedHistory = ProcessHistory.Create(notification.SubjectRef, notification.ActivityId);
                    completedHistory.Update(ProcessStatus.Completed, notification.PostMoodId);
                    _dbContext.ProcessHistories.Add(completedHistory);
                    break;

                case ProcessStatus.Skipped:
                    stepProgress.Skip(notification.PostMoodId);
                    break;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
