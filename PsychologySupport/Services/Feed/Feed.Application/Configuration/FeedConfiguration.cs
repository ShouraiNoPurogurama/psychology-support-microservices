namespace Feed.Application.Configuration;

public sealed class FeedConfiguration
{
    public int DefaultPageSize { get; set; } = 20;
    public int MaxPageSize { get; set; } = 100;
    public int VipFeedDays { get; set; } = 7;
    public int TrendingTopN { get; set; } = 99;
    public int FeedShardCount { get; set; } = 4;
    public VipCriteria VipCriteria { get; set; } = new();
    public CacheSettings Cache { get; set; } = new();
}

public sealed class VipCriteria
{
    public int MinFollowers { get; set; } = 5000;
    public int MinDailyActiveMinutes { get; set; } = 60;
    public string MinSubscriptionTier { get; set; } = "GOLD";
}

public sealed class CacheSettings
{
    public TimeSpan VipCacheTtl { get; set; } = TimeSpan.FromMinutes(15);
    public TimeSpan IdempotencyTtl { get; set; } = TimeSpan.FromMinutes(10);
    public TimeSpan SnapshotTtl { get; set; } = TimeSpan.FromMinutes(30);
}

public sealed class CursorConfiguration
{
    public string HmacSecret { get; set; } = string.Empty;
}

public sealed class RedisConfiguration
{
    public string KeyPrefix { get; set; } = "feed";
    public int RetryCount { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(100);
}
