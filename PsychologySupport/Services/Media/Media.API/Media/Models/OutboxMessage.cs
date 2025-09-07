using System;
using System.Collections.Generic;

namespace Media.API.Media.Models;

public partial class OutboxMessage
{
    public Guid Id { get; set; }

    public Guid AggregateId { get; set; }

    public string Type { get; set; } = null!;

    public string Payload { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public Guid? CreatedAtActor { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }
}
