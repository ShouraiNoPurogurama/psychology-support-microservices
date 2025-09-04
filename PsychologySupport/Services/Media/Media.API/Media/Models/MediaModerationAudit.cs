using System;
using System.Collections.Generic;

namespace Media.API.Media.Models;

public partial class MediaModerationAudit
{
    public Guid Id { get; set; }

    public Guid MediaId { get; set; }

    public string Status { get; set; } = null!;

    public decimal? Score { get; set; }

    public string PolicyVersion { get; set; } = null!;

    public string RawJson { get; set; } = null!;

    public DateTime CheckedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual MediaAsset Media { get; set; } = null!;
}
