using BuildingBlocks.DDD;

namespace Alias.API.Models.Public;

public partial class AliasAudit : IEntity<Guid>, IHasCreationAudit
{
    public Guid Id { get; set; }

    public Guid AliasId { get; set; }

    public string Action { get; set; } = null!;

    public string? Details { get; set; }
    
    public DateTimeOffset? CreatedAt { get; set; }
    
    public string? CreatedBy { get; set; }
}
