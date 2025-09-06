namespace Post.Domain.Models;

public partial class CategoryTag
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string? Color { get; set; }

    public bool IsActive { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastModified { get; set; }

    public string? UnicodeCodepoint { get; set; }
}
