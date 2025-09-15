using Post.Domain.Enums;
using Post.Domain.Exceptions;

namespace Post.Domain.Aggregates.Post.ValueObjects;

public sealed record AuthorInfo
{
    public Guid AliasId { get; init; }
    public Guid AliasVersionId { get; init; }

    // EF Core materialization
    private AuthorInfo() { }

    // Ctor kín – chỉ factory được phép gọi
    private AuthorInfo(Guid aliasId, Guid? aliasVersionId)
    {
        AliasId = aliasId;
        AliasVersionId = aliasVersionId ?? Guid.Empty;
    }

    public static AuthorInfo Create(Guid aliasId, Guid? aliasVersionId = null)
    {
        if (aliasId == Guid.Empty)
            throw new InvalidPostDataException("ID của tác giả không hợp lệ.");

        return new AuthorInfo(aliasId, aliasVersionId);
    }

    public bool HasVersionInfo => AliasVersionId != Guid.Empty;
}

public sealed record ModerationInfo
{
    public ModerationStatus Status { get; init; }
    public IReadOnlyList<string> Reasons { get; init; } = [];
    public string? PolicyVersion { get; init; }
    public DateTimeOffset? ModeratedAt { get; init; }

    // EF Core materialization
    private ModerationInfo() { }

    // Ctor kín – dùng cho factory & transitions
    private ModerationInfo(
        ModerationStatus status,
        IReadOnlyList<string> reasons,
        string? policyVersion,
        DateTimeOffset? moderatedAt)
    {
        Status = status;
        Reasons = reasons;
        PolicyVersion = policyVersion;
        ModeratedAt = moderatedAt;
    }

    public static ModerationInfo Pending()
        => new(ModerationStatus.Pending, [], null, null);

    public ModerationInfo Approve(string policyVersion)
    {
        if (Status != ModerationStatus.Pending)
            throw new InvalidPostModerationStateException("Chỉ các bài viết đang chờ duyệt mới có thể được phê duyệt.");

        return new ModerationInfo(
            ModerationStatus.Approved,
            [],
            policyVersion,
            DateTimeOffset.UtcNow);
    }

    public ModerationInfo Reject(List<string> reasons, string policyVersion)
    {
        if (Status != ModerationStatus.Pending)
            throw new InvalidPostModerationStateException("Chỉ các bài viết đang chờ duyệt mới có thể được từ chối.");

        if (reasons is null || reasons.Count == 0)
            throw new InvalidPostModerationStateException("Phải cung cấp ít nhất một lý do từ chối duyệt.");

        return new ModerationInfo(
            ModerationStatus.Rejected,
            reasons.AsReadOnly(),
            policyVersion,
            DateTimeOffset.UtcNow);
    }

    public bool IsPending  => Status == ModerationStatus.Pending;
    public bool IsApproved => Status == ModerationStatus.Approved;
    public bool IsRejected => Status == ModerationStatus.Rejected;
}
