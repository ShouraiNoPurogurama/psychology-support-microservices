namespace Feed.Infrastructure.Data.Redis;

public static class RedisKeyPatterns
{
    private const string VIP_PREFIX = "vip";
    private const string RANK_PREFIX = "rank"; 
    private const string TRENDING_PREFIX = "trending";
    private const string IDEM_PREFIX = "idem";
    private const string SNAPSHOT_PREFIX = "feed:reg:snap";
    
    public static string VipStatus(Guid aliasId) => $"{VIP_PREFIX}:{aliasId}";
    public static string PostRank(Guid postId) => $"{RANK_PREFIX}:{postId}";
    public static string TrendingDaily(DateOnly date) => $"{TRENDING_PREFIX}:{date:yyyyMMdd}";
    public static string IdempotencyKey(string hashedKey) => $"{IDEM_PREFIX}:{hashedKey}";
    public static string FeedSnapshot(Guid aliasId, long snapshotTs) => $"{SNAPSHOT_PREFIX}:{aliasId}:{snapshotTs}";
}

public enum RankField
{
    Score,
    Reactions, 
    Comments,
    Ctr,
    UpdatedAt
}

public static class RankFieldNames
{
    public const string Score = "score";
    public const string Reactions = "reactions";
    public const string Comments = "comments";
    public const string Ctr = "ctr";
    public const string UpdatedAt = "updated_at";
    
    public static string GetFieldName(RankField field) => field switch
    {
        RankField.Score => Score,
        RankField.Reactions => Reactions,
        RankField.Comments => Comments,
        RankField.Ctr => Ctr,
        RankField.UpdatedAt => UpdatedAt,
        _ => throw new ArgumentOutOfRangeException(nameof(field))
    };
}
