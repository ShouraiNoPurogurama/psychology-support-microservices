using BuildingBlocks.DDD;

namespace Profile.API.Models.Pii;

public partial class AliasOwnerMap : Entity<Guid>, IHasCreationAudit
{
    public Guid AliasId { get; set; }

    public Guid UserId { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public PersonProfile PersonProfile { get; set; }
}
