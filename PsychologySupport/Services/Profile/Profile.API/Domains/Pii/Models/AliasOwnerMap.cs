using BuildingBlocks.DDD;

namespace Profile.API.Domains.Pii.Models;

public partial class AliasOwnerMap : Entity<Guid>, IHasCreationAudit
{
    public Guid AliasId { get; set; }

    public Guid UserId { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public PersonProfile PersonProfile { get; set; }
}
