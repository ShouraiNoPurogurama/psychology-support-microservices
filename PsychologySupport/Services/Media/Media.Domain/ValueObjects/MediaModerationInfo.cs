using Media.Domain.Enums;
using Media.Domain.Exceptions;

namespace Media.Domain.ValueObjects;

public sealed record MediaModerationInfo
{
    public MediaModerationStatus Status { get; init; }
    public decimal? Score { get; init; }
    public DateTimeOffset? CheckedAt { get; init; }
    public string? PolicyVersion { get; init; }
    public string? RawJson { get; init; }

    private MediaModerationInfo() { }

    private MediaModerationInfo(
        MediaModerationStatus status,
        decimal? score = null,
        DateTimeOffset? checkedAt = null,
        string? policyVersion = null,
        string? rawJson = null)
    {
        Status = status;
        Score = score;
        CheckedAt = checkedAt;
        PolicyVersion = policyVersion;
        RawJson = rawJson;
    }

    public static MediaModerationInfo Pending()
        => new(MediaModerationStatus.Pending);

    public static MediaModerationInfo Approve(string policyVersion, decimal? score = null, string? rawJson = null)
    {
        if (string.IsNullOrWhiteSpace(policyVersion))
            throw new MediaDomainException("Policy version is required for approval.");

        return new MediaModerationInfo(
            MediaModerationStatus.Approved,
            score,
            DateTimeOffset.UtcNow,
            policyVersion,
            rawJson);
    }

    public static MediaModerationInfo Reject(string policyVersion, decimal? score = null, string? rawJson = null)
    {
        if (string.IsNullOrWhiteSpace(policyVersion))
            throw new MediaDomainException("Policy version is required for rejection.");

        return new MediaModerationInfo(
            MediaModerationStatus.Rejected,
            score,
            DateTimeOffset.UtcNow,
            policyVersion,
            rawJson);
    }

    public static MediaModerationInfo Flag(string policyVersion, decimal? score = null, string? rawJson = null)
    {
        if (string.IsNullOrWhiteSpace(policyVersion))
            throw new MediaDomainException("Policy version is required for flagging.");

        return new MediaModerationInfo(
            MediaModerationStatus.Flagged,
            score,
            DateTimeOffset.UtcNow,
            policyVersion,
            rawJson);
    }

    public bool IsApproved => Status == MediaModerationStatus.Approved;
    public bool IsRejected => Status == MediaModerationStatus.Rejected;
    public bool IsPending => Status == MediaModerationStatus.Pending;
    public bool IsFlagged => Status == MediaModerationStatus.Flagged;
    public bool RequiresReview => Status == MediaModerationStatus.Pending || Status == MediaModerationStatus.Flagged;
}
