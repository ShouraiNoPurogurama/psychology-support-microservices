using System;
using System.Collections.Generic;

namespace Wellness.Domain.Models;

public partial class ChallengeStep
{
    public Guid Id { get; set; }

    public Guid? ChallengeId { get; set; }

    public Guid? ActivityId { get; set; }

    public int DayNumber { get; set; }

    public int OrderIndex { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual Activity? Activity { get; set; }

    public virtual Challenge? Challenge { get; set; }

    public virtual ICollection<ChallengeStepProgress> ChallengeStepProgresses { get; set; } = new List<ChallengeStepProgress>();
}
