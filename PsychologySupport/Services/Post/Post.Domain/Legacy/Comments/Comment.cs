namespace Post.Domain.Legacy.Comments;

public partial class Comment : AggregateRoot<Guid>, ISoftDeletable
{
    public Guid PostId { get; private set; }

    public Guid? ParentCommentId { get; private set; }

    public string Path { get; private set; } = null!;

    public int Level { get; private set; }

    public string Content { get; private set; } = null!;

    public Guid AuthorAliasId { get; private set; }

    public Guid AuthorAliasVersionId { get; private set; }
    public string ModerationStatus { get; private set; } = null!;
    
    
    public bool IsDeleted { get; set; }
    
    public DateTimeOffset? DeletedAt { get;  set; }
    
    public string? DeletedByAliasId { get; set; }
}