using System;
using System.Collections.Generic;

namespace Billing.API.Domains.Billings.Models;

public partial class IdempotencyKey
{
    public Guid Id { get; set; }

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
