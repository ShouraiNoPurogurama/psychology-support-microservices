using System;
using System.Collections.Generic;

namespace Wellness.Domain.Models;

public partial class Activity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string ActivityType { get; set; } = null!;

    public int? Duration { get; set; }

    public string? Instructions { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual ICollection<ChallengeStep> ChallengeSteps { get; set; } = new List<ChallengeStep>();

    public virtual ICollection<ProcessHistory> ProcessHistories { get; set; } = new List<ProcessHistory>();
}
