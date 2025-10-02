namespace Post.Domain.Aggregates.OutboxMessages;

public partial class OutboxMessage: Entity<Guid>
{
    public string Type { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTimeOffset OccurredOn { get; set; }
    public DateTimeOffset? ProcessedOn { get; set; }
}
