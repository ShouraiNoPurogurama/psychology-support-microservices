namespace Post.Domain.Models;

public partial class Comment
{
    public Guid Id { get; set; }

    public Guid PostId { get; set; }

    public Guid? ParentCommentId { get; set; }

    public string Path { get; set; } = null!;

    public int Level { get; set; }

    public string Content { get; set; } = null!;

    public Guid AuthorAliasId { get; set; }

    public Guid AuthorAliasVersionId { get; set; }

    public string ModerationStatus { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public DateTime? DeletedAt { get; set; }
}
