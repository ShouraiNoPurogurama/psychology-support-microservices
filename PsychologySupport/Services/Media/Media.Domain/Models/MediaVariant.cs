using Media.Domain.Abstractions;
using Media.Domain.Enums;

namespace Media.Domain.Models;

public partial class MediaVariant : AuditableEntity<Guid>
{
    public Guid MediaId { get; set; }

    public VariantType VariantType { get; set; }

    public MediaFormat Format { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public long Bytes { get; set; }

    public string BucketKey { get; set; } = null!;

    public string? CdnUrl { get; set; }

    public virtual MediaAsset Media { get; set; } = null!;
}
