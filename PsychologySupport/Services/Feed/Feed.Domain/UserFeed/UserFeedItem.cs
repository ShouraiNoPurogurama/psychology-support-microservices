namespace Feed.Domain.UserFeed;

public sealed class UserFeedItem
{
    public Guid AliasId { get; }
    public DateOnly YmdBucket { get; }
    public short Shard { get; }
    public sbyte RankBucket { get; }
    public long RankI64 { get; }
    public Guid TsUuid { get; }
    public Guid PostId { get; }
    public DateTimeOffset? CreatedAt { get; }

    private UserFeedItem(
        Guid aliasId, 
        DateOnly ymdBucket, 
        short shard, 
        sbyte rankBucket, 
        long rankI64, 
        Guid tsUuid, 
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
        CreatedAt = createdAtUtc?.ToUniversalTime();
    }

    public static UserFeedItem Create(
        Guid aliasId,
        Guid postId,
        DateOnly? ymdBucket = null,
        short shard = 0,
        sbyte rankBucket = 0,
        long rankI64 = 0,
        Guid? tsUuid = null,
        DateTimeOffset? createdAt = null)
    {
        var tsUuidValue = tsUuid ?? Guid.NewGuid();
        var ymdBucketValue = ymdBucket ?? DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);
        
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
}
