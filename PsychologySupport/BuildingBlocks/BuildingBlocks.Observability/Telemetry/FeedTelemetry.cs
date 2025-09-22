using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace BuildingBlocks.Observability.Telemetry;

public static class FeedActivitySource
{
    public static readonly ActivitySource Instance = new("Feed.Service");
    
    public static class Operations
    {
        public const string GetVipFeed = "feed.get_vip_feed";
        public const string GetRegularFeed = "feed.get_regular_feed"; 
        public const string FanOutToVip = "feed.fanout_vip";
        public const string UpdateRanking = "feed.update_ranking";
        public const string CassandraQuery = "feed.cassandra_query";
        public const string RedisOperation = "feed.redis_operation";
    }
}

public static class FeedMetrics
{
    private static readonly Meter _meter = new("Feed.Service");
    
    public static readonly Counter<long> FeedRequests = _meter.CreateCounter<long>(
        "feed_requests_total", 
        description: "Total feed requests");
        
    public static readonly Histogram<double> FeedDuration = _meter.CreateHistogram<double>(
        "feed_request_duration_ms",
        description: "Feed request duration in milliseconds");
        
    public static readonly Counter<long> CassandraOperations = _meter.CreateCounter<long>(
        "cassandra_operations_total",
        description: "Total Cassandra operations");
        
    public static readonly Counter<long> RedisOperations = _meter.CreateCounter<long>(
        "redis_operations_total",
        description: "Total Redis operations");
        
    public static readonly Gauge<long> VipUsersActive = _meter.CreateGauge<long>(
        "vip_users_active",
        description: "Currently active VIP users");
}
