using Cassandra;

namespace Feed.Domain.UserFeed;

public sealed class UserFeedItem
{
    public Guid AliasId { get; }
    public LocalDate YmdBucket { get; }
    public short Shard { get; }
    public sbyte RankBucket { get; }
    public long RankI64 { get; }
    public TimeUuid TsUuid { get; }
    public Guid PostId { get; }
    public DateTimeOffset? CreatedAt { get; }

    private UserFeedItem(
        Guid aliasId, 
        LocalDate ymdBucket, 
        short shard, 
        sbyte rankBucket, 
        long rankI64, 
        TimeUuid tsUuid, 
        Guid postId, 
        DateTimeOffset? createdAtUtc)
    {
        if (aliasId == Guid.Empty)
            throw new ArgumentException("AliasId is required", nameof(aliasId));
        if (postId == Guid.Empty)
            throw new ArgumentException("PostId is required", nameof(postId));

        AliasId = aliasId;
        YmdBucket = ymdBucket;
        Shard = shard;
        RankBucket = rankBucket;
        RankI64 = rankI64;
        TsUuid = tsUuid;
        PostId = postId;
        CreatedAt = createdAtUtc?.Kind != DateTimeKind.Utc ? createdAtUtc?.ToUniversalTime() : createdAtUtc;
    }

    public static UserFeedItem Create(
        Guid aliasId,
        Guid postId,
        LocalDate? ymdBucket = null,
        short shard = 0,
        sbyte rankBucket = 0,
        long rankI64 = 0,
        TimeUuid? tsUuid = null,
        DateTimeOffset? createdAt = null)
    {
        var tsUuidValue = tsUuid ?? TimeUuid.NewId();
        var ymdBucketValue = ymdBucket ?? LocalDate.FromDateTime(DateTime.UtcNow.Date);
        
        return new(
            aliasId,
            ymdBucketValue,
            shard,
            rankBucket,
            rankI64,
            tsUuidValue,
            postId,
            createdAt);
    }

    public DateTimeOffset GetTimestampDateTime()
        => TsUuid.GetDate();
}
