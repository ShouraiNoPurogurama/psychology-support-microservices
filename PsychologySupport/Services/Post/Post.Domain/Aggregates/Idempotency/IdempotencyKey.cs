namespace Post.Domain.Aggregates.Idempotency;

public partial class IdempotencyKey : Entity<Guid>, IHasCreationAudit
{
    public Guid Id { get; set; }
    public Guid Key { get; set; }

    public string RequestHash { get; set; } = null!;
    public string? ResponsePayload { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }    
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public Guid CreatedByAliasId { get; set; }
}
