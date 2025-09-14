namespace Billing.Domain.Models;

public partial class InvoiceItem : AuditableEntity<Guid>
{
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

    public virtual InvoiceSnapshot InvoiceSnapshot { get; set; } = null!;
}
