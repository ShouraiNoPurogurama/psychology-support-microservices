using System;
using System.Collections.Generic;

namespace Billing.API.Domains.Billings.Models;

public partial class Invoice
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public Guid OrderId { get; set; }

    public Guid Subject_ref { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime IssuedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid LastModifiedBy { get; set; }

    public virtual InvoiceSnapshot? InvoiceSnapshot { get; set; }

    public virtual Order Order { get; set; } = null!;
}
