namespace Media.Domain.Models;

public partial class IdempotencyKey : Entity<Guid>, IHasCreationAudit
{
    public Guid Key { get; set; }

    public string RequestHash { get; set; } = null!;

    public string? ResponsePayload { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }


}
