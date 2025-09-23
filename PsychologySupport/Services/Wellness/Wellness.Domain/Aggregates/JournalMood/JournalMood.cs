using System;
using System.Collections.Generic;
using Wellness.Domain.Abstractions;

namespace Wellness.Domain.Aggregates.JournalMood;

public partial class JournalMood : AuditableEntity<Guid>
{
    public Guid SubjectRef { get; set; }

    public Guid MoodId { get; set; }

    public string? Note { get; set; }

    // Navigation
    public Mood Mood { get; set; } = null!;
}
