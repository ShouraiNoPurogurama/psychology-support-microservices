using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Domain.Aggregates.Posts.ValueObjects;

public record ModerationInfo
{
    public ModerationStatus Status { get; init; }
    public List<string> Reasons { get; init; } = new();
    public string? PolicyVersion { get; init; }
    public DateTimeOffset? EvaluatedAt { get; init; }

    public bool IsApproved => Status == ModerationStatus.Approved;
    public bool IsRejected => Status == ModerationStatus.Rejected;
    public bool IsPending => Status == ModerationStatus.Pending;
    public bool IsFlagged => Status == ModerationStatus.Flagged;

    private ModerationInfo() { }

    public static ModerationInfo Pending()
    {
        return new ModerationInfo
        {
            Status = ModerationStatus.Pending,
            Reasons = new List<string>()
        };
    }

    public ModerationInfo Approve(string policyVersion)
    {
        return this with
        {
            Status = ModerationStatus.Approved,
            PolicyVersion = policyVersion,
            EvaluatedAt = DateTimeOffset.UtcNow,
            Reasons = new List<string>()
        };
    }

    public ModerationInfo Reject(List<string> reasons, string policyVersion)
    {
        return this with
        {
            Status = ModerationStatus.Rejected,
            PolicyVersion = policyVersion,
            EvaluatedAt = DateTimeOffset.UtcNow,
            Reasons = reasons
        };
    }

    public ModerationInfo Flag(List<string> reasons, string policyVersion)
    {
        return this with
        {
            Status = ModerationStatus.Flagged,
            PolicyVersion = policyVersion,
            EvaluatedAt = DateTimeOffset.UtcNow,
            Reasons = reasons
        };
    }
}
