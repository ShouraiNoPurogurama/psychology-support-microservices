namespace Post.Domain.Models.Public;

public partial class Comment : SoftDeletableEntity<Guid>
{
    public Guid PostId { get; set; }

    public Guid? ParentCommentId { get; set; }

    public string Path { get; set; } = null!;

    public int Level { get; set; }

    public string Content { get; set; } = null!;

    public Guid AuthorAliasId { get; set; }

    public Guid AuthorAliasVersionId { get; set; }
    public string ModerationStatus { get; set; } = null!;
}