using System;
using System.Collections.Generic;
using Wellness.Domain.Abstractions;


namespace Wellness.Domain.Aggregates.Challenges;

public partial class ChallengeStep : AuditableEntity<Guid>
{
    public Guid? ChallengeId { get; set; }

    public Guid ActivityId { get; set; }

    public int DayNumber { get; set; } // Ngày thứ bao nhiêu trong thử thách

    public int OrderIndex { get; set; } // Thứ tự thực hiện trong ngày (nếu có nhiều hoạt động trong một ngày)

    public virtual Activity? Activity { get; set; }

    public virtual Challenge? Challenge { get; set; }

    public virtual ICollection<ChallengeStepProgress> ChallengeStepProgresses { get; set; } = new List<ChallengeStepProgress>();
}
