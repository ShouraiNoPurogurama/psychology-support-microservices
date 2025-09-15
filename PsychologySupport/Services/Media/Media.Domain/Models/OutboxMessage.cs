using Media.Domain.Abstractions;

namespace Media.Domain.Models;

public partial class OutboxMessage : AuditableEntity<Guid>
{
    public Guid AggregateId { get; set; }

    public string EventType { get; set; } = null!;

    public string Payload { get; set; } = null!;

    public DateTime OccurredOn { get; set; }

    public DateTime? ProcessedOn { get; set; }

}
