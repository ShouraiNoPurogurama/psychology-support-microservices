namespace Billing.API.Models;

public partial class Invoice : AggregateRoot<Guid>
{
    public string Code { get; set; } = null!;

    public Guid OrderId { get; set; }

    public Guid SubjectRef { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime IssuedAt { get; set; }

    public virtual InvoiceSnapshot? InvoiceSnapshot { get; set; }

    public virtual Order Order { get; set; } = null!;
}
