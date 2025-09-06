namespace Post.Domain.Models;

public partial class PostMedium
{
    public Guid Id { get; set; }

    public Guid PostId { get; set; }

    public Guid MediaId { get; set; }

    public int Position { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }
}
