using System;
using System.Collections.Generic;
using Wellness.Domain.Abstractions;
using Wellness.Domain.Enums;

namespace Wellness.Domain.Aggregates.ModuleSection;

public partial class ModuleProgress : AuditableEntity<Guid>
{
    public Guid SubjectRef { get; set; }

    public Guid SectionId { get; set; }

    public ProcessStatus ProcessStatus { get; set; }

    public int? MinutesRead { get; set; } // tổng số phút đã đọc trong section này

    public virtual ICollection<ArticleProgress> ArticleProgresses { get; set; } = new List<ArticleProgress>();

    public virtual ModuleSection? Section { get; set; }
}
