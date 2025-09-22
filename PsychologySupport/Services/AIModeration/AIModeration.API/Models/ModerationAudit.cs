using BuildingBlocks.DDD;

namespace AIModeration.API.Models;

public partial class ModerationAudit : Entity<Guid>, IHasCreationAudit
{
    public Guid Id { get; set; }

    public Guid ItemId { get; set; }

    public string Event { get; set; } = null!;

    public string? Data { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public virtual ModerationItem Item { get; set; } = null!;
}
