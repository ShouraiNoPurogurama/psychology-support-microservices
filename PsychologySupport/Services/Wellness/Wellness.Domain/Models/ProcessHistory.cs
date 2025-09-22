using System;
using System.Collections.Generic;

namespace Wellness.Domain.Models;

public partial class ProcessHistory
{
    public Guid Id { get; set; }

    public Guid SubjectRef { get; set; }

    public Guid? ActivityId { get; set; }

    public Guid? ChallengeId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string ProcessStatus { get; set; } = null!;

    public string? PreMood { get; set; }

    public string? PostMood { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual Activity? Activity { get; set; }

    public virtual Challenge? Challenge { get; set; }
}
