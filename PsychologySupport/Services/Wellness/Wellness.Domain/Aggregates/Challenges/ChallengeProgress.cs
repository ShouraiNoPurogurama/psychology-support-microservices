using System;
using System.Collections.Generic;
using Wellness.Domain.Abstractions;
using Wellness.Domain.Enums;

namespace Wellness.Domain.Aggregates.Challenges;

public partial class ChallengeProgress : AuditableEntity<Guid>
{
    public Guid SubjectRef { get; set; }

    public Guid? ChallengeId { get; set; }

    public ProcessStatus ProcessStatus { get; set; }

    public int? ProgressPercent { get; set; } // Phần trăm tiến độ hoàn thành thử thách(0-100)

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }

    public virtual Challenge? Challenge { get; set; }

    public virtual ICollection<ChallengeStepProgress> ChallengeStepProgresses { get; set; } = new List<ChallengeStepProgress>();
}
