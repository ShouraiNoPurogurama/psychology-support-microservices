using Feed.Domain.PostReplica;
using Feed.Infrastructure.Persistence.Cassandra.Models;
using Feed.Infrastructure.Persistence.Cassandra.Utils;

namespace Feed.Infrastructure.Persistence.Cassandra.Mappings;

/// <summary>
/// Mapper for PostReplica domain model to/from Cassandra row models.
/// </summary>
public static class PostReplicaMapper
{
    // PostReplica mappings
    public static PostReplicaRow ToRow(PostReplica domain) => new()
    {
        YmdBucket = CassandraTypeMapper.ToLocalDate(domain.YmdBucket),
        CreatedAt = CassandraTypeMapper.ToTimeUuid(domain.CreatedAt),
        PostId = domain.PostId,
        AuthorAliasId = domain.AuthorAliasId,
        Visibility = domain.Visibility,
        Status = domain.Status
    };

    public static PostReplica ToDomain(PostReplicaRow row)
        => PostReplica.Create(
            row.PostId,
            row.AuthorAliasId,
            row.Visibility,
            row.Status,
            CassandraTypeMapper.ToDateOnly(row.YmdBucket),
            CassandraTypeMapper.ToDateTimeOffset(row.CreatedAt)
        );

    // PostPublicFinalizedByDay mappings
    public static PostPublicFinalizedByDayRow ToRow(PostPublicFinalizedByDay domain) => new()
    {
        YmdBucket = CassandraTypeMapper.ToLocalDate(domain.YmdBucket),
        CreatedAt = CassandraTypeMapper.ToTimeUuid(domain.CreatedAt),
        PostId = domain.PostId,
        AuthorAliasId = domain.AuthorAliasId
    };

    public static PostPublicFinalizedByDay ToDomainPublicFinalized(PostPublicFinalizedByDayRow row)
        => PostPublicFinalizedByDay.Create(
            row.PostId,
            row.AuthorAliasId,
            CassandraTypeMapper.ToDateOnly(row.YmdBucket),
            CassandraTypeMapper.ToDateTimeOffset(row.CreatedAt)
        );

    // PostReplicaById mappings
    public static PostReplicaByIdRow ToRow(PostReplicaById domain) => new()
    {
        PostId = domain.PostId,
        YmdBucket = CassandraTypeMapper.ToLocalDate(domain.YmdBucket),
        CreatedAt = CassandraTypeMapper.ToTimeUuid(domain.CreatedAt)
    };

    public static PostReplicaById ToDomainById(PostReplicaByIdRow row)
        => PostReplicaById.Create(
            row.PostId,
            CassandraTypeMapper.ToDateOnly(row.YmdBucket),
            CassandraTypeMapper.ToDateTimeOffset(row.CreatedAt)
        );
}
