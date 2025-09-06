namespace Post.Domain.Models;

public partial class Post : SoftDeletableEntity<Guid>
{

    public string Visibility { get; set; } = null!;

    public string? Title { get; set; }

    public string Content { get; set; } = null!;

    public Guid AuthorAliasId { get; set; }

    public Guid AuthorAliasVersionId { get; set; }

    public string ModerationStatus { get; set; } = null!;

    public List<string> ModerationReasons { get; set; } = null!;

    public string? ModerationPolicyVersion { get; set; }

    public int ReactionCount { get; set; }

    public int CommentCount { get; set; }
}
