using System;
using System.Collections.Generic;
using Wellness.Domain.Abstractions;
using Wellness.Domain.Aggregates.JournalMood;
using Wellness.Domain.Enums;
using Wellness.Domain.Models;

namespace Wellness.Domain.Aggregates.Challenge;

public partial class ChallengeStepProgress : AuditableEntity<Guid>
{
    public Guid? ChallengeProgressId { get; set; }

    public Guid? ChallengeStepId { get; set; }

    public ProcessStatus ProcessStatus { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public Guid? PostMoodId { get; set; } // Log cảm xúc sau khi hoàn thành hoạt động

    public virtual Mood? PostMood { get; set; }

    public virtual ChallengeProgress? ChallengeProgress { get; set; }

    public virtual ChallengeStep? ChallengeStep { get; set; }
}
