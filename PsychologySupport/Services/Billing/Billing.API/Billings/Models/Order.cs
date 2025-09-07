using System;
using System.Collections.Generic;

namespace Billing.API.Billings.Models;

public partial class Order
{
    public Guid Id { get; set; }

    public Guid AliasId { get; set; }

    public string OrderType { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string? PromoCode { get; set; }

    public string Status { get; set; } = null!;

    public Guid? PaymentId { get; set; }

    public Guid? InvoiceId { get; set; }

    public Guid? IdempotencyKeyId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid LastModifiedBy { get; set; }

    public virtual IdempotencyKey? IdempotencyKey { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
