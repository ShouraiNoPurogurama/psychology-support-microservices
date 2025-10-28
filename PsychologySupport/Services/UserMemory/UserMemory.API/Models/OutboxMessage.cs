namespace UserMemory.API.Models;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTimeOffset OccurredOn { get; set; }
    public DateTimeOffset? ProcessedOn { get; set; }
}