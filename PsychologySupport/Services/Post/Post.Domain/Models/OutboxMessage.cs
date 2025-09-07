namespace Post.Domain.Models;

public partial class OutboxMessage: Entity<Guid>
{
    public string Type { get; set; } = null!;

    public string Content { get; set; } = null!;
    public DateTime OccuredOn { get; set; }
    public DateTime? ProcessedOn { get; set; }
}
