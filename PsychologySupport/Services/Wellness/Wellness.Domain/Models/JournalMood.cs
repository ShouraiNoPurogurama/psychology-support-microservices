using System;
using System.Collections.Generic;

namespace Wellness.Domain.Models;

public partial class JournalMood
{
    public Guid Id { get; set; }

    public Guid SubjectRef { get; set; }

    public string Mood { get; set; } = null!;

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }
}
