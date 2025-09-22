namespace Billing.Domain.Models;

public partial class InvoiceSnapshot : AuditableEntity<Guid>
{
    public Guid InvoiceId { get; set; }

    public string OrderType { get; set; } = null!;

    public decimal? TotalDiscountAmount { get; set; }

    public string AliasInfo { get; set; } = null!;

    public string Currency { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public decimal? TaxAmount { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
