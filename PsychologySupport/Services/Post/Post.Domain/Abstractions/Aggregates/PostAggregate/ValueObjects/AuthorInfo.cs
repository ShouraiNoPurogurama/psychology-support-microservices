using Post.Domain.Enums;
using Post.Domain.Exceptions;

namespace Post.Domain.Abstractions.Aggregates.PostAggregate.ValueObjects;

public sealed record AuthorInfo
{
    public Guid AliasId { get; init; }
    public Guid? AliasVersionId { get; init; }

    public AuthorInfo(Guid aliasId, Guid? aliasVersionId = null)
    {
        if (aliasId == Guid.Empty)
            throw new InvalidPostDataException("ID của tác giả không hợp lệ.");

        AliasId = aliasId;
        AliasVersionId = aliasVersionId;
    }

    public bool HasVersionInfo => AliasVersionId.HasValue && AliasVersionId != Guid.Empty;
}

public sealed record ModerationInfo
{
    public ModerationStatus Status { get; init; }
    public IReadOnlyList<string> Reasons { get; init; }
    public string? PolicyVersion { get; init; }
    public DateTimeOffset? ModeratedAt { get; init; }

    public ModerationInfo(ModerationStatus status = ModerationStatus.Pending)
    {
        Status = status;
        Reasons = new List<string>();
        PolicyVersion = null;
        ModeratedAt = null;
    }

    private ModerationInfo(ModerationStatus status, IReadOnlyList<string> reasons, string? policyVersion, DateTimeOffset? moderatedAt)
    {
        Status = status;
        Reasons = reasons;
        PolicyVersion = policyVersion;
        ModeratedAt = moderatedAt;
    }

    public ModerationInfo Approve(string policyVersion)
    {
        if (Status != ModerationStatus.Pending)
            throw new InvalidPostModerationStateException("Chỉ các bài viết đang chờ duyệt mới có thể được phê duyệt.");

        return new ModerationInfo(ModerationStatus.Approved, new List<string>(), policyVersion, DateTimeOffset.UtcNow);
    }

    public ModerationInfo Reject(List<string> reasons, string policyVersion)
    {
        if (Status != ModerationStatus.Pending)
            throw new InvalidPostModerationStateException("Chỉ các bài viết đang chờ duyệt mới có thể được từ chối.");

        if (reasons == null || reasons.Count == 0)
            throw new InvalidPostModerationStateException("Phải cung cấp ít nhất một lý do từ chối duyệt.");

        return new ModerationInfo(ModerationStatus.Rejected, reasons.AsReadOnly(), policyVersion, DateTimeOffset.UtcNow);
    }

    public bool IsPending => Status == ModerationStatus.Pending;
    public bool IsApproved => Status == ModerationStatus.Approved;
    public bool IsRejected => Status == ModerationStatus.Rejected;
}
