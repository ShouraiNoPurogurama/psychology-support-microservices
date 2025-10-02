using MediatR;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
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

            if (notification.Status == ProcessStatus.Completed)
            {
                // Tạo ProcessHistory mới khi hoàn thành step
                var completedHistory = ProcessHistory.Create(notification.SubjectRef, notification.ActivityId);
                completedHistory.Update(ProcessStatus.Completed, notification.PostMoodId);

                _dbContext.ProcessHistories.Add(completedHistory);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
