using System;
using System.Collections.Generic;

namespace Wellness.Domain.Models;

public partial class Challenge
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Difficulty { get; set; } = null!;

    public string ChallengeType { get; set; } = null!;

    public int DurationActivity { get; set; }

    public int DurationDate { get; set; }

    public Guid? ModuleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual ICollection<ChallengeProgress> ChallengeProgresses { get; set; } = new List<ChallengeProgress>();

    public virtual ICollection<ChallengeStep> ChallengeSteps { get; set; } = new List<ChallengeStep>();

    public virtual WellnessModule? Module { get; set; }

    public virtual ICollection<ProcessHistory> ProcessHistories { get; set; } = new List<ProcessHistory>();
}
