namespace Post.Domain.Posts;

public partial class PostMedia : SoftDeletableEntity<Guid>
{
    public Guid PostId { get; set; }

    public Guid MediaId { get; set; }
    public int? Position { get; set; }
}