namespace Post.Domain.Models;

public partial class PostCounterDelta
{
    public Guid Id { get; set; }

    public Guid PostId { get; set; }

    public string Kind { get; set; } = null!;

    public short Delta { get; set; }

    public DateTime OccuredAt { get; set; }

    public bool Processed { get; set; }
}
