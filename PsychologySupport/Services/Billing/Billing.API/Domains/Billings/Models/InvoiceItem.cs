using System;
using System.Collections.Generic;

namespace Billing.API.Domains.Billings.Models;

public partial class InvoiceItem
{
    public Guid Id { get; set; }

    public Guid InvoiceSnapshotId { get; set; }

    public string ItemType { get; set; } = null!;

    public string ProductCode { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string? PromoCode { get; set; }

    public string? Description { get; set; }

    public int Quantity { get; set; }

    public string Unit { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid LastModifiedBy { get; set; }

    public virtual InvoiceSnapshot InvoiceSnapshot { get; set; } = null!;
}
