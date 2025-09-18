using Media.Domain.Abstractions;
using Media.Domain.Enums;

namespace Media.Domain.Models;

public partial class MediaModerationAudit : AuditableEntity<Guid>
{
    public Guid MediaId { get; set; }

    public MediaModerationStatus Status { get; set; }

    public decimal? Score { get; set; }

    public string? PolicyVersion { get; set; } = null!;

    public string? RawJson { get; set; } = null!;

    public DateTimeOffset? CheckedAt { get; set; }

    public virtual MediaAsset Media { get; set; } = null!;
}
