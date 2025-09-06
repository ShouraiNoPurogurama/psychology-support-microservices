namespace Post.Domain.Models;

public partial class PostCategory
{
    public Guid Id { get; set; }

    public Guid PostId { get; set; }

    public Guid CategoryTagId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedByAliasId { get; set; }

    public DateTime? DeletedAt { get; set; }
}
