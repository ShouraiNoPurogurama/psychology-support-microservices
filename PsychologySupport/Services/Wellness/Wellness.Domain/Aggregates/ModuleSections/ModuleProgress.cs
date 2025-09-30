using Wellness.Domain.Abstractions;
using Wellness.Domain.Enums;
using static Wellness.Domain.Events.ModuleSectionDomainEvents;

namespace Wellness.Domain.Aggregates.ModuleSections;

public partial class ModuleProgress : AggregateRoot<Guid>
{
    public Guid SubjectRef { get; set; }
    public Guid SectionId { get; set; } // Khóa ngoại tham chiếu đến ModuleSection
    public ProcessStatus ProcessStatus { get; set; }
    public int? MinutesRead { get; set; } // tổng số phút đã đọc trong section này

    public virtual ICollection<ArticleProgress> ArticleProgresses { get; set; } = new List<ArticleProgress>();
    public virtual ModuleSection? Section { get; set; }


    public static ModuleProgress Create(
    Guid subjectRef,
    Guid sectionId,
    List<Guid> articleIds,
    Guid firstSectionArticleId,
    int firstArticleDuration
)
    {
        var moduleProgress = new ModuleProgress
        {
            Id = Guid.NewGuid(),
            SubjectRef = subjectRef,
            SectionId = sectionId,
            ProcessStatus = ProcessStatus.Progressing,
            MinutesRead = firstArticleDuration // dùng duration của bài viết đầu tiên
        };

        moduleProgress.AddDomainEvent(new ModuleProgressCreatedEvent(
            ModuleProgressId: moduleProgress.Id,
            SubjectRef: subjectRef,
            SectionId: sectionId,
            ArticleIds: articleIds,
            FirstSectionArticleId: firstSectionArticleId
        ));

        return moduleProgress;
    }

    public void Update(Guid articleId, int minutesRead)
    {
        MinutesRead = (MinutesRead ?? 0) + minutesRead;

        AddDomainEvent(new ModuleProgressUpdatedEvent(
            ModuleProgressId: this.Id,
            SubjectRef: this.SubjectRef,
            SectionId: this.SectionId,
            ArticleId: articleId,
            MinutesRead: minutesRead
        ));
    }
}
