using Post.Domain.Abstractions;

namespace Post.Infrastructure.Resilience.Entities;

public partial class IdempotencyKey : Entity<Guid>, IHasCreationAudit
{
    public Guid Id { get; set; }
    public Guid Key { get; set; }

    public string RequestHash { get; set; } = null!;
    public string? ResponsePayload { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }    
    
    public DateTimeOffset? CreatedAt { get; set; }
    
    public string? CreatedByAliasId { get; set; }
}
