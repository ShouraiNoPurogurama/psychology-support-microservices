using System;
using System.Collections.Generic;

namespace Wellness.Domain.Models;

public partial class SectionArticle
{
    public Guid Id { get; set; }

    public Guid? SectionId { get; set; }

    public string Title { get; set; } = null!;

    public string ContentJson { get; set; } = null!;

    public int OrderIndex { get; set; }

    public int? Duration { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual ICollection<ArticleProgress> ArticleProgresses { get; set; } = new List<ArticleProgress>();

    public virtual ModuleSection? Section { get; set; }
}
