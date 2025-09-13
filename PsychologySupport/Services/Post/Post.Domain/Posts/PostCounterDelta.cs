namespace Post.Domain.Posts;

public partial class PostCounterDelta : Entity<Guid>
{
    public Guid PostId { get; set; }

    public string Kind { get; set; } = null!;

    public short Delta { get; set; }
    public DateTime OccuredAt { get; set; }
    public bool Processed { get; set; }
}
