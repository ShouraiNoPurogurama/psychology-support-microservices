using Wellness.Domain.Abstractions;
using Wellness.Domain.Enums;
using Wellness.Domain.Aggregates.Challenges;
using Wellness.Domain.Aggregates.JournalMoods;

namespace Wellness.Domain.Aggregates.ProcessHistories;

public partial class ProcessHistory : AuditableEntity<Guid>
{
    public Guid SubjectRef { get; private set; }

    public Guid? ActivityId { get; private set; }

    public DateTimeOffset StartTime { get; private set; }

    public DateTimeOffset? EndTime { get; private set; }

    public ProcessStatus ProcessStatus { get; private set; }

    public Guid? PostMoodId { get; private set; }

    public virtual Activity? Activity { get; private set; }

    public virtual Mood? PostMood { get; private set; }

    private ProcessHistory() { }

    private ProcessHistory(Guid subjectRef, Guid activityId)
    {
        Id = Guid.NewGuid();
        SubjectRef = subjectRef;
        ActivityId = activityId;
        ProcessStatus = ProcessStatus.Progressing;
        StartTime = DateTimeOffset.UtcNow.AddHours(7);
    }

    public static ProcessHistory Create(Guid subjectRef, Guid activityId)
    {
        var history = new ProcessHistory(subjectRef, activityId);

        return history;
    }

    public void Update(ProcessStatus status, Guid? postMoodId = null)
    {
        ProcessStatus = status;

        if (status is ProcessStatus.Completed or ProcessStatus.Skipped)
        {
            EndTime = DateTimeOffset.UtcNow.AddHours(7);
        }

        if (status == ProcessStatus.Completed)
        {
            PostMoodId = postMoodId;
        }
    }
}
