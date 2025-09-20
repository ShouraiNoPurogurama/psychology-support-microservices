using Feed.Domain.UserPinning;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Mappings;

public static class UserPinningMapper
{
    public static UserPinnedPostsRow ToRow(UserPinnedPost domain) => new()
    {
        AliasId = domain.AliasId,
        PinnedAt = domain.PinnedAt,
        PostId = domain.PostId
    };

    public static UserPinnedPost ToDomain(UserPinnedPostsRow row)
        => UserPinnedPost.Create(
            row.AliasId,
            row.PostId,
            row.PinnedAt
        );
}
