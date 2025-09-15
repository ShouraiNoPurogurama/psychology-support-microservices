using Media.Domain.Abstractions;
using Media.Domain.Enums;

namespace Media.Domain.Models;

public partial class MediaAsset : AggregateRoot<Guid>
{
    public MediaState State { get; set; }

    public string SourceMime { get; set; } = null!;

    public long SourceBytes { get; set; }

    public string ChecksumSha256 { get; set; } = null!;


    // Optional (images, deduplication)
    public string? Phash64 { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }


    // Security / policy
    public bool ExifRemoved { get; set; } = false;
    public bool HoldThumbUntilPass { get; set; } = false;


    // Moderation (nullable -> optional)
    public string? ModerationStatus { get; set; }

    public decimal? ModerationScore { get; set; }

    public DateTime? ModerationCheckedAt { get; set; }

    public string? ModerationPolicyVersion { get; set; }

    public string? RawModerationJson { get; set; }

    public virtual ICollection<MediaModerationAudit> MediaModerationAudits { get; set; } = new List<MediaModerationAudit>();

    public virtual ICollection<MediaOwner> MediaOwners { get; set; } = new List<MediaOwner>();

    public virtual ICollection<MediaProcessingJob> MediaProcessingJobs { get; set; } = new List<MediaProcessingJob>();

    public virtual ICollection<MediaVariant> MediaVariants { get; set; } = new List<MediaVariant>();
}
