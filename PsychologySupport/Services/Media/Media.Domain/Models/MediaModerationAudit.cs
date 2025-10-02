using Media.Domain.Enums;

namespace Media.Domain.Models;

public sealed class MediaModerationAudit : AuditableEntity<Guid>
{
    public Guid MediaId { get; private set; }
    public MediaModerationStatus Status { get; private set; }
    public decimal? Score { get; private set; }
    public string? PolicyVersion { get; private set; }
    public string? RawJson { get; private set; }
    public DateTimeOffset? CheckedAt { get; private set; }

    public MediaAsset Media { get; private set; } = null!;

    // EF Core constructor
    private MediaModerationAudit() { }

    // Factory method
    public static MediaModerationAudit Create(
        Guid mediaId,
        MediaModerationStatus status,
        decimal? score = null,
        string? policyVersion = null,
        string? rawJson = null)
    {
        return new MediaModerationAudit
        {
            Id = Guid.NewGuid(),
            MediaId = mediaId,
            Status = status,
            Score = score,
            PolicyVersion = policyVersion,
            RawJson = rawJson,
            CheckedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
    
    public void UpdateStatus(MediaModerationStatus status, decimal? score = null, string? policyVersion = null, string? rawJson = null)
    {
        Status = status;
        Score = score;
        PolicyVersion = policyVersion;
        RawJson = rawJson;
        CheckedAt = DateTimeOffset.UtcNow;
    }
}
