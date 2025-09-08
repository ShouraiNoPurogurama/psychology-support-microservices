using BuildingBlocks.DDD;
using System;
using System.Collections.Generic;

namespace Billing.API.Models;

public partial class IdempotencyKey : Entity<Guid>
{
    public Guid Key { get; set; }

    public string RequestHash { get; set; } = null!;

    public string? ResponsePayload { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid LastModifiedBy { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
