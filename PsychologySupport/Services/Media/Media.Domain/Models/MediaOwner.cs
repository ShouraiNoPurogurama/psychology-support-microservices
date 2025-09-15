using Media.Domain.Abstractions;
using Media.Domain.Enums;

namespace Media.Domain.Models;

public partial class MediaOwner : AuditableEntity<Guid>
{
    public Guid MediaId { get; set; }

    public MediaOwnerType MediaOwnerType { get; set; }

    public Guid MediaOwnerId { get; set; }

    public virtual MediaAsset Media { get; set; } = null!;
}
