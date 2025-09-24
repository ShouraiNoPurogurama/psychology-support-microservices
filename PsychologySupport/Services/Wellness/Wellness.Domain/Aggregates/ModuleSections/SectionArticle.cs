using Wellness.Domain.Abstractions;
using Wellness.Domain.Aggregates.ModuleSections.ValueObjects;

namespace Wellness.Domain.Aggregates.ModuleSections;

public partial class SectionArticle : AuditableEntity<Guid>
{
    public Guid? SectionId { get; set; }

    public string Title { get; set; } = null!;

    public Guid MediaId { get; set; }

    public string ContentJson { get; set; } = null!; // Lưu trữ nội dung bài viết dưới dạng JSON

    public int OrderIndex { get; set; }

    public int Duration { get; set; } // Thời gian ước tính để đọc bài viết (tính bằng phút)

    public ArticleSource Source { get; set; } 

    public virtual ICollection<ArticleProgress> ArticleProgresses { get; set; } = new List<ArticleProgress>();

    public virtual ModuleSection? Section { get; set; }
}
