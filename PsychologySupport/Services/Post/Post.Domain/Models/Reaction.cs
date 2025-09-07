namespace Post.Domain.Models;

public sealed class Reaction : SoftDeletableEntity<Guid>
{
    public string TargetType { get; set; } = null!;  // "post" | "comment" | ...
    public Guid TargetId { get; set; }

    //Lưu code tránh FK chéo service. Nếu muốn FK cứng thì đổi sang ReactionTypeId.
    public string ReactionType { get; set; } = null!; //"care", "haha" (validate ở API)
    
    public Guid AuthorAliasId { get; set; }
    
    public Guid AuthorAliasVersionId { get; set; }
}