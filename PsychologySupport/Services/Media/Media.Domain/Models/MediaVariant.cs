using System;
using System.Collections.Generic;

namespace Media.API.Media.Models;

public partial class MediaVariant
{
    public Guid Id { get; set; }

    public Guid MediaId { get; set; }

    public string VariantType { get; set; } = null!;

    public string Format { get; set; } = null!;

    public int Width { get; set; }

    public int Height { get; set; }

    public long Bytes { get; set; }

    public string BucketKey { get; set; } = null!;

    public string? CdnUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual MediaAsset Media { get; set; } = null!;
}
