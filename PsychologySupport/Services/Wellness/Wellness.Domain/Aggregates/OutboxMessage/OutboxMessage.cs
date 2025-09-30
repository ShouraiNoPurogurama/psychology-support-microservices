using System;
using System.Collections.Generic;

namespace Wellness.Domain.Aggregates.OutboxMessage;

public partial class OutboxMessage
{
    public Guid Id { get; set; }

    public string AggregateType { get; set; } = null!;

    public Guid AggregateId { get; set; }

    public string EventType { get; set; } = null!;

    public string Payload { get; set; } = null!;

    public DateTime OccurredOn { get; set; }

    public DateTime? ProcessedOn { get; set; }
}
