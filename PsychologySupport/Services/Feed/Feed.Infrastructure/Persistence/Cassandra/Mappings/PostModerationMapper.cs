using Feed.Domain.PostModeration;
using Feed.Infrastructure.Persistence.Cassandra.Models;

namespace Feed.Infrastructure.Persistence.Cassandra.Mappings;

public static class PostModerationMapper
{
    public static PostSuppressedRow ToRow(PostSuppression domain) => new()
    {
        PostId = domain.PostId,
        Reason = domain.Reason,
        SuppressedAt = domain.SuppressedAt,
        SuppressedUntil = domain.SuppressedUntil
    };

    public static PostSuppression ToDomain(PostSuppressedRow row)
        => PostSuppression.Create(
            row.PostId,
            row.Reason,
            row.SuppressedAt ?? DateTimeOffset.UtcNow,
            row.SuppressedUntil
        );
}
