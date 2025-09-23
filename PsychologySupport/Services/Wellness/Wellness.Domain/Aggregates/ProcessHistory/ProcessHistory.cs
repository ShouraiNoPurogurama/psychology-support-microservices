using Wellness.Domain.Abstractions;
using Wellness.Domain.Aggregates.Challenge;
using Wellness.Domain.Aggregates.JournalMood;
using Wellness.Domain.Enums;

namespace Wellness.Domain.Aggregates.ProcessHistory;

public partial class ProcessHistory : AuditableEntity<Guid>
{
    public Guid SubjectRef { get; set; }

    public Guid? ActivityId { get; set; }

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }

    public ProcessStatus ProcessStatus { get; set; }

    public Guid? PostMoodId { get; set; } // Log cảm xúc sau khi hoàn thành hoạt động

    public virtual Activity? Activity { get; set; }

    public virtual Mood? PostMood { get; set; }


}
