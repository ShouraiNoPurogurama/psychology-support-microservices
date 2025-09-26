using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Wellness.Application.Data;
using Wellness.Domain.Aggregates.Challenges;
using Wellness.Domain.Enums;

namespace Wellness.Application.Features.Challenges.Commands;

public record CreateChallengeProgressCommand(
    Guid IdempotencyKey,
    Guid SubjectRef,
    Guid ChallengeId
) : IdempotentCommand<CreateChallengeProgressResult>(IdempotencyKey);

public record CreateChallengeProgressResult(
    Guid ChallengeProgressId,
    ProcessStatus Status,
    int ProgressPercent
);

internal class CreateChallengeProgressHandler
    : ICommandHandler<CreateChallengeProgressCommand, CreateChallengeProgressResult>
{
    private readonly IWellnessDbContext _dbContext;

    public CreateChallengeProgressHandler(IWellnessDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateChallengeProgressResult> Handle(CreateChallengeProgressCommand request, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.ChallengeProgresses
        .Where(cp => cp.SubjectRef == request.SubjectRef
                     && cp.ChallengeId == request.ChallengeId
                     && cp.ProcessStatus == ProcessStatus.Progressing)
        .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            throw new CustomValidationException(new Dictionary<string, string[]>
            {
                ["CHALLENGE_PROGRESS_EXISTS"] = new[] { "ChallengeProgress is already in progress." }
            });
        }


        // 2. Load challenge
        var challenge = await _dbContext.Challenges
            .Include(c => c.ChallengeSteps)
            .FirstOrDefaultAsync(c => c.Id == request.ChallengeId, cancellationToken);

        if (challenge == null)
            throw new NotFoundException($"Challenge with Id = {request.ChallengeId} not found.");

        // 3. Create ChallengeProgress via domain method
        var progress = ChallengeProgress.Create(request.SubjectRef, challenge);

        _dbContext.ChallengeProgresses.Add(progress);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateChallengeProgressResult(
            progress.Id,
            progress.ProcessStatus,
            progress.ProgressPercent
        );
    }
}
