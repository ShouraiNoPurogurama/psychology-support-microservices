using BuildingBlocks.DDD;

namespace Profile.API.Pii_Old.Models;

public partial class AliasOwnerMap : Entity<Guid>, IHasCreationAudit
{
    public Guid AliasId { get; set; }

    public Guid UserId { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
