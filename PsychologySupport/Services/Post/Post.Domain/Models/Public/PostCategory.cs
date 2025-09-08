namespace Post.Domain.Models.Public;

public partial class PostCategory : SoftDeletableEntity<Guid>
{
    public Guid PostId { get; set; }
    public Guid CategoryTagId { get; set; }
}