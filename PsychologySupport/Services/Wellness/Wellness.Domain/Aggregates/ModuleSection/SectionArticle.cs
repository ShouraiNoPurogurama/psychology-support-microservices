using Wellness.Domain.Abstractions;

namespace Wellness.Domain.Aggregates.ModuleSection;

public partial class SectionArticle : AuditableEntity<Guid>
{
    public Guid? SectionId { get; set; }

    public string Title { get; set; } = null!;

    public Guid MediaId { get; set; }

    public string ContentJson { get; set; } = null!; // Lưu trữ nội dung bài viết dưới dạng JSON

    public int OrderIndex { get; set; }

    public int Duration { get; set; } // Thời gian ước tính để đọc bài viết (tính bằng phút)

    public virtual ICollection<ArticleProgress> ArticleProgresses { get; set; } = new List<ArticleProgress>();

    public virtual ModuleSection? Section { get; set; }
}
