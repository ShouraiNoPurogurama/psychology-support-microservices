using Wellness.Domain.Abstractions;
using Wellness.Domain.Enums;
using static Wellness.Domain.Events.ChallengeDomainEvents;

namespace Wellness.Domain.Aggregates.Challenges;

public partial class ChallengeProgress : AggregateRoot<Guid>
{
    public Guid SubjectRef { get; private set; }

    public Guid ChallengeId { get; private set; }

    public ProcessStatus ProcessStatus { get; private set; }

    public int ProgressPercent { get; private set; } // 0-100

    public DateTimeOffset StartDate { get; private set; }

    public DateTimeOffset? EndDate { get; private set; }

    public virtual Challenge Challenge { get; private set; }

    public virtual ICollection<ChallengeStepProgress> ChallengeStepProgresses { get; private set; }
        = new List<ChallengeStepProgress>();

    private ChallengeProgress() { }

    private ChallengeProgress(Guid subjectRef, Guid challengeId)
    {
        Id = Guid.NewGuid();
        SubjectRef = subjectRef;
        ChallengeId = challengeId;
        ProcessStatus = ProcessStatus.Progressing;
        ProgressPercent = 0;
        StartDate = DateTimeOffset.UtcNow;
    }

    public static ChallengeProgress Create(Guid subjectRef, Challenge challenge)
    {
        var progress = new ChallengeProgress(subjectRef, challenge.Id);

        progress.AddDomainEvent(new ChallengeProgressCreatedEvent(
            ChallengeProgressId: progress.Id,
            SubjectRef: subjectRef,
            ChallengeId: challenge.Id,
            StepIds: challenge.ChallengeSteps.Select(s => s.Id).ToList()
        ));

        return progress;
    }


    public void UpdateStep(Guid stepId, ProcessStatus stepStatus, Guid? postMoodId = null)
    {
        // Tìm hoặc tạo mới StepProgress
        var stepProgress = ChallengeStepProgresses.FirstOrDefault(x => x.ChallengeStepId == stepId);
        if (stepProgress is null)
        {
            stepProgress = ChallengeStepProgress.Create(this.Id, stepId);
            ChallengeStepProgresses.Add(stepProgress);
        }

        // Cập nhật trạng thái của step
        switch (stepStatus)
        {
            case ProcessStatus.Completed:
                stepProgress.Complete(postMoodId);
                break;
            case ProcessStatus.Progressing:
                stepProgress.Start();
                break;
            case ProcessStatus.Skipped:
                stepProgress.Skip(postMoodId);
                break;
        }

        // Tính lại tiến độ dựa trên trạng thái đã cập nhật
        var totalSteps = Challenge?.ChallengeSteps.Count ?? ChallengeStepProgresses.Count;
        var completedSteps = ChallengeStepProgresses.Count(x => x.ProcessStatus == ProcessStatus.Completed);

        ProgressPercent = totalSteps > 0
            ? (int)Math.Round((completedSteps * 100.0) / totalSteps)
            : 0;

        if (ProgressPercent >= 100)
        {
            ProcessStatus = ProcessStatus.Completed;
            EndDate = DateTimeOffset.UtcNow;
        }
        else if (completedSteps > 0)
        {
            ProcessStatus = ProcessStatus.Progressing;
        }

        // Raise domain event
        AddDomainEvent(new ChallengeProgressUpdatedEvent(
            ChallengeProgressId: this.Id,
            SubjectRef: this.SubjectRef,
            ChallengeStepId: stepId,
            ActivityId: this.Challenge.ChallengeSteps
                .First(s => s.Id == stepId)
                .ActivityId,
            PostMoodId: postMoodId,
            Status: stepStatus
        ));
    }

}
