using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wellness.Application.Data;
using Wellness.Domain.Aggregates.Challenges;
using static Wellness.Domain.Events.ChallengeDomainEvents;

namespace Wellness.Application.Features.Challenges.EventHandler.Domains
{
    public class ChallengeProgressCreatedEventHandler
        : INotificationHandler<ChallengeProgressCreatedEvent>
    {
        private readonly IWellnessDbContext _dbContext;

        public ChallengeProgressCreatedEventHandler(IWellnessDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(ChallengeProgressCreatedEvent notification, CancellationToken cancellationToken)
        {
            foreach (var stepId in notification.StepIds)
            {
                var stepProgress = ChallengeStepProgress.Create(notification.ChallengeProgressId, stepId);

                // Step đầu tiên thì bắt đầu luôn
                if (stepId == notification.StepIds.First())
                {
                    stepProgress.Start();
                }

                _dbContext.ChallengeStepProgresses.Add(stepProgress);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
