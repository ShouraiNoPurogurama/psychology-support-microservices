namespace Billing.Domain.Models;

public partial class OutboxMessage : AuditableEntity<Guid>
{
    public string AggregateType { get; set; } = null!;

    public Guid AggregateId { get; set; }

    public string EventType { get; set; } = null!;

    public string Payload { get; set; } = null!;

    public DateTimeOffset OccurredOn { get; set; }

    public DateTimeOffset? ProcessedOn { get; set; }

}
