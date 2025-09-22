using System;
using System.Collections.Generic;

namespace Wellness.Domain.Models;

public partial class ArticleProgress
{
    public Guid Id { get; set; }

    public Guid? ModuleProgressId { get; set; }

    public Guid? ArticleId { get; set; }

    public string ProcessStatus { get; set; } = null!;

    public DateTime LogDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual SectionArticle? Article { get; set; }

    public virtual ModuleProgress? ModuleProgress { get; set; }
}
