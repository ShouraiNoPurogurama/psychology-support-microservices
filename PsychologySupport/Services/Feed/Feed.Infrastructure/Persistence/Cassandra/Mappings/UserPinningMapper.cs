using Feed.Domain.UserPinning;
using Feed.Infrastructure.Persistence.Cassandra.Models;
using Feed.Infrastructure.Persistence.Cassandra.Utils;

namespace Feed.Infrastructure.Persistence.Cassandra.Mappings;

public static class UserPinningMapper
{
    public static UserPinnedPostsRow ToRow(UserPinnedPost domain) => new()
    {
        AliasId = domain.AliasId,
        PinnedAt = CassandraTypeMapper.ToTimeUuid(domain.PinnedAt),
        PostId = domain.PostId
    };

    public static UserPinnedPost ToDomain(UserPinnedPostsRow row)
        => UserPinnedPost.Create(
            row.AliasId,
            row.PostId,
            CassandraTypeMapper.ToGuid(row.PinnedAt)
        );
}
