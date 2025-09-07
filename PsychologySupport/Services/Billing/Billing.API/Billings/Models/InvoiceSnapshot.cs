using System;
using System.Collections.Generic;

namespace Billing.API.Billings.Models;

public partial class InvoiceSnapshot
{
    public Guid Id { get; set; }

    public Guid InvoiceId { get; set; }

    public string OrderType { get; set; } = null!;

    public decimal? TotalDiscountAmount { get; set; }

    public string AliasInfo { get; set; } = null!;

    public string Currency { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public decimal? TaxAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid LastModifiedBy { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
