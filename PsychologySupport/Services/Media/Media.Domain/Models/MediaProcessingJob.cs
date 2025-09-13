using Media.Domain.Abstractions;
using Media.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Media.API.Media.Models;

public partial class MediaProcessingJob : AuditableEntity<Guid>
{
    public Guid MediaId { get; set; }

    public JobType JobType { get; set; }

    public ProcessStatus Status { get; set; }

    public int Attempt { get; set; }

    public DateTime? NextRetryAt { get; set; }

    public string? ErrorMessage { get; set; }

    public virtual MediaAsset Media { get; set; } = null!;
}
