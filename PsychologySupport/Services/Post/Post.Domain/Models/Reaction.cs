namespace Post.Domain.Models;

public partial class Reaction
{
    public Guid Id { get; set; }

    public string TargetType { get; set; } = null!;

    public Guid TargetId { get; set; }

    public string ReactionType { get; set; } = null!;

    public Guid AuthorAliasId { get; set; }

    public Guid AuthorAliasVersionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }
}
