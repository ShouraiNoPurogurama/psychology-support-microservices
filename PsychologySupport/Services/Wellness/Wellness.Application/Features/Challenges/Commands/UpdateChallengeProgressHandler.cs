using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Application.Exceptions;
using Wellness.Domain.Enums;

namespace Wellness.Application.Features.Challenges.Commands
{
    public record UpdateChallengeProgressCommand(
        Guid SubjectRef,
        Guid ChallengeProgressId,
        Guid StepId,
        ProcessStatus StepStatus,
        Guid? PostMoodId
    ) : ICommand<UpdateChallengeProgressResult>;

    public record UpdateChallengeProgressResult(
        Guid ChallengeProgressId,
        ProcessStatus ProcessStatus,
        int ProgressPercent
    );

    internal class UpdateChallengeProgressHandler
        : ICommandHandler<UpdateChallengeProgressCommand, UpdateChallengeProgressResult>
    {
        private readonly IWellnessDbContext _dbContext;

        public UpdateChallengeProgressHandler(IWellnessDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UpdateChallengeProgressResult> Handle(UpdateChallengeProgressCommand request, CancellationToken cancellationToken)
        {
            var challengeProgress = await _dbContext.ChallengeProgresses
                .Include(cp => cp.ChallengeStepProgresses)
                .Include(cp => cp.Challenge)
                .ThenInclude(c => c.ChallengeSteps)
                .FirstOrDefaultAsync(
                    cp => cp.Id == request.ChallengeProgressId,
                    cancellationToken);

            if (challengeProgress == null)
                throw new WellnessNotFoundException($"Không tìm thấy ChallengeProgress với Id = {request.ChallengeProgressId}.");

            // Cập nhật step
            challengeProgress.UpdateStep(request.StepId, request.StepStatus, request.PostMoodId);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new UpdateChallengeProgressResult(
                challengeProgress.Id,
                challengeProgress.ProcessStatus,
                challengeProgress.ProgressPercent
            );
        }
    }
}
