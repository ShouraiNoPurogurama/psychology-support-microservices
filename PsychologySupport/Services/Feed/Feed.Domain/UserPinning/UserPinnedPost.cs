using System;

namespace Feed.Domain.UserPinning;

public sealed class UserPinnedPost
{
    public Guid AliasId { get; }
    public Guid PinnedAt { get; }
    public Guid PostId { get; }

    private UserPinnedPost(Guid aliasId, Guid pinnedAt, Guid postId)
    {
        if (aliasId == Guid.Empty)
            throw new ArgumentException("AliasId is required", nameof(aliasId));
        if (postId == Guid.Empty)
            throw new ArgumentException("PostId is required", nameof(postId));

        AliasId = aliasId;
        PinnedAt = pinnedAt;
        PostId = postId;
    }

    public static UserPinnedPost Create(Guid aliasId, Guid postId, Guid? pinnedAt = null)
        => new(aliasId, pinnedAt ?? Guid.NewGuid(), postId);
}
