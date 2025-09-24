using System;
using Wellness.Domain.Abstractions;
using Wellness.Domain.Enums;

namespace Wellness.Domain.Aggregates.ModuleSections;

public partial class ArticleProgress : AuditableEntity<Guid>
{
    public Guid ModuleProgressId { get; private set; }
    public Guid ArticleId { get; private set; }
    public ProcessStatus ProcessStatus { get; private set; }
    public DateTimeOffset? LogDate { get; private set; }

    public virtual SectionArticle? Article { get; private set; }
    public virtual ModuleProgress? ModuleProgress { get; private set; }

    private ArticleProgress() { } 

    private ArticleProgress(Guid moduleProgressId, Guid articleId)
    {
        Id = Guid.NewGuid();
        ModuleProgressId = moduleProgressId;
        ArticleId = articleId;
        ProcessStatus = ProcessStatus.NotStarted;
    }

    public static ArticleProgress Create(Guid moduleProgressId, Guid articleId)
        => new ArticleProgress(moduleProgressId, articleId);

    public void Update(ProcessStatus status)
    {
        ProcessStatus = status;
        if (status == ProcessStatus.Completed)
        {
            LogDate = DateTimeOffset.UtcNow;
        }
    }
}
