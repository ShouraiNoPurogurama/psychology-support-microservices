using System;
using System.Collections.Generic;

namespace Wellness.Domain.Models;

public partial class ChallengeProgress
{
    public Guid Id { get; set; }

    public Guid SubjectRef { get; set; }

    public Guid? ChallengeId { get; set; }

    public string ProcessStatus { get; set; } = null!;

    public int? ProgressPercent { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual Challenge? Challenge { get; set; }

    public virtual ICollection<ChallengeStepProgress> ChallengeStepProgresses { get; set; } = new List<ChallengeStepProgress>();
}
