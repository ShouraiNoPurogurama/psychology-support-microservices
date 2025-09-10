namespace Billing.API.Models;

public partial class IdempotencyKey : Entity<Guid>, IHasCreationAudit
{
    public Guid Key { get; set; }

    public string RequestHash { get; set; } = null!;

    public string? ResponsePayload { get; set; }

    public DateTime ExpiresAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public DateTimeOffset? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }

 
}
