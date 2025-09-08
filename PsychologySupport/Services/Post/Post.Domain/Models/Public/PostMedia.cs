namespace Post.Domain.Models.Public;

public partial class PostMedia : SoftDeletableEntity<Guid>
{
    public Guid PostId { get; set; }

    public Guid MediaId { get; set; }
    public int? Position { get; set; }
}