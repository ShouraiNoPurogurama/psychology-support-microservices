using System;
using System.Collections.Generic;
using Wellness.Domain.Abstractions;

namespace Wellness.Domain.Aggregates.ModuleSection;

public partial class ModuleSection : AuditableEntity<Guid>
{
    public Guid? ModuleId { get; set; }

    public string Title { get; set; } = null!;

    public Guid MediaId { get; set; }

    public string? Description { get; set; }

    public int TotalDuration { get; set; } // Tổng thời gian ước tính để hoàn thành section (tính bằng phút)

    public virtual WellnessModule? Module { get; set; }

    public virtual ICollection<ModuleProgress> ModuleProgresses { get; set; } = new List<ModuleProgress>();

    public virtual ICollection<SectionArticle> SectionArticles { get; set; } = new List<SectionArticle>();
}
