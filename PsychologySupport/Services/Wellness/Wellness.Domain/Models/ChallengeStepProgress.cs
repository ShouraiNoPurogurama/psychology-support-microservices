using System;
using System.Collections.Generic;

namespace Wellness.Domain.Models;

public partial class ChallengeStepProgress
{
    public Guid Id { get; set; }

    public Guid? ChallengeProgressId { get; set; }

    public Guid? ChallengeStepId { get; set; }

    public string ProcessStatus { get; set; } = null!;

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? PreMood { get; set; }

    public string? PostMood { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual ChallengeProgress? ChallengeProgress { get; set; }

    public virtual ChallengeStep? ChallengeStep { get; set; }
}
