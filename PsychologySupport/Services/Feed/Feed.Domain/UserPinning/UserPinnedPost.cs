using Cassandra;

namespace Feed.Domain.UserPinning;

public sealed class UserPinnedPost
{
    public Guid AliasId { get; }
    public TimeUuid PinnedAt { get; }
    public Guid PostId { get; }

    private UserPinnedPost(Guid aliasId, TimeUuid pinnedAt, Guid postId)
    {
        if (aliasId == Guid.Empty)
            throw new ArgumentException("AliasId is required", nameof(aliasId));
        if (postId == Guid.Empty)
            throw new ArgumentException("PostId is required", nameof(postId));

        AliasId = aliasId;
        PinnedAt = pinnedAt;
        PostId = postId;
    }

    public static UserPinnedPost Create(Guid aliasId, Guid postId, TimeUuid? pinnedAt = null)
        => new(aliasId, pinnedAt ?? TimeUuid.NewId(), postId);

    public DateTimeOffset GetPinnedAtDateTime()
        => PinnedAt.GetDate();
}
