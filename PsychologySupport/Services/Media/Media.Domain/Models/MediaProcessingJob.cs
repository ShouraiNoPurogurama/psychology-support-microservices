using System;
using System.Collections.Generic;

namespace Media.API.Media.Models;

public partial class MediaProcessingJob
{
    public Guid Id { get; set; }

    public Guid MediaId { get; set; }

    public string JobType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int Attempt { get; set; }

    public DateTime? NextRetryAt { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual MediaAsset Media { get; set; } = null!;
}
