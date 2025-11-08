using Wellness.Domain.Abstractions;
using Wellness.Domain.Aggregates.JournalMoods;
using Wellness.Domain.Enums;

namespace Wellness.Domain.Aggregates.Challenges;

public partial class ChallengeStepProgress : AuditableEntity<Guid>
{
    public Guid ChallengeProgressId { get; private set; }

    public Guid ChallengeStepId { get; private set; }

    public ProcessStatus ProcessStatus { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public Guid? PostMoodId { get; private set; }

    public virtual Mood? PostMood { get; private set; }

    public virtual ChallengeProgress? ChallengeProgress { get; private set; }

    public virtual ChallengeStep? ChallengeStep { get; private set; }

    private ChallengeStepProgress() { }

    private ChallengeStepProgress(Guid challengeProgressId, Guid challengeStepId)
    {
        Id = Guid.NewGuid();
        ChallengeProgressId = challengeProgressId;
        ChallengeStepId = challengeStepId;
        ProcessStatus = ProcessStatus.NotStarted;
    }

    public static ChallengeStepProgress Create(Guid challengeProgressId, Guid stepId)
        => new ChallengeStepProgress(challengeProgressId, stepId);

    public void Start()
    {
        if (ProcessStatus == ProcessStatus.NotStarted)
        {
            ProcessStatus = ProcessStatus.Progressing;
            StartedAt = DateTimeOffset.UtcNow;
        }
    }

    public void Complete(Guid? postMoodId = null)
    {
        ProcessStatus = ProcessStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        PostMoodId = postMoodId;
    }

    public void Skip(Guid? postMoodId = null)
    {
        ProcessStatus = ProcessStatus.Skipped;
        CompletedAt = DateTimeOffset.UtcNow;
        PostMoodId = postMoodId;
    }
}
