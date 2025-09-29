using System;
using System.Collections.Generic;

namespace Wellness.Domain.Models;

public partial class ModuleSection
{
    public Guid Id { get; set; }

    public Guid? ModuleId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? TotalDuration { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual WellnessModule? Module { get; set; }

    public virtual ICollection<ModuleProgress> ModuleProgresses { get; set; } = new List<ModuleProgress>();

    public virtual ICollection<SectionArticle> SectionArticles { get; set; } = new List<SectionArticle>();
}
