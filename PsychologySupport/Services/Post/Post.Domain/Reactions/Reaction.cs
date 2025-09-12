
namespace Post.Domain.Reactions;

public sealed class Reaction : AggregateRoot<Guid>, ISoftDeletable
{
    public string TargetType { get; private set; } = null!;  // "post" | "comment" | ...
    public Guid TargetId { get; private set; }

    //Lưu code tránh FK chéo service. Nếu muốn FK cứng thì đổi sang ReactionTypeId.
    public string ReactionType { get; private set; } = null!; //"care", "haha" (validate ở API)
    
    public Guid AuthorAliasId { get; private set; }
    
    public Guid AuthorAliasVersionId { get; private set; }
    
    public bool IsDeleted { get; set; }
    
    public DateTimeOffset? DeletedAt { get; set; }
    
    public string? DeletedByAliasId { get; set; }
}