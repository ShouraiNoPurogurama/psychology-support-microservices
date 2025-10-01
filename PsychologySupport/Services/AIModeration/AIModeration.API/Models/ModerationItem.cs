
using BuildingBlocks.DDD;

namespace AIModeration.API.Models;

public partial class ModerationItem : AuditableEntity<Guid>
{
    public string TargetType { get; set; } = null!;

    public Guid TargetId { get; set; }

    public string? ContentHash { get; set; }

    public string Status { get; set; } = null!;

    public string PolicyVersion { get; set; } = null!;

    public string? Scores { get; set; }

    public List<string>? Reasons { get; set; }

    public string? DecidedBy { get; set; }

    public DateTimeOffset? DecidedAt { get; set; }

    public virtual ICollection<ModerationAudit> ModerationAudits { get; set; } = new List<ModerationAudit>();
}
