using System;
using System.Collections.Generic;
using Wellness.Domain.Abstractions;
using Wellness.Domain.Enums;

namespace Wellness.Domain.Aggregates.ModuleSection;

public partial class ArticleProgress : AuditableEntity<Guid>
{
    public Guid? ModuleProgressId { get; set; }

    public Guid? ArticleId { get; set; }

    public ProcessStatus ProcessStatus { get; set; }

    public DateTimeOffset? LogDate { get; set; } // log thời gian hoàn thành đọc bài viết

    public virtual SectionArticle? Article { get; set; }

    public virtual ModuleProgress? ModuleProgress { get; set; }
}
