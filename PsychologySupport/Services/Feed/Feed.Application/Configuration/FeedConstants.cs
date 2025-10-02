namespace Feed.Application.Configuration;

/// <summary>
/// Constants for Feed service configuration and tuning parameters.
/// Centralizes magic numbers with documentation.
/// </summary>
public static class FeedConstants
{
    /// <summary>
    /// Batch size for Cassandra write operations.
    /// Prevents connection pool exhaustion while maintaining throughput.
    /// Based on Cassandra driver connection pool size and max requests per connection.
    /// </summary>
    public const int CASSANDRA_BATCH_SIZE = 100;

    /// <summary>
    /// Default page size for feed requests.
    /// Balances freshness, network payload, and user experience.
    /// </summary>
    public const int DEFAULT_PAGE_SIZE = 20;

    /// <summary>
    /// Minimum allowed page size for feed requests.
    /// Prevents excessive pagination overhead.
    /// </summary>
    public const int MIN_PAGE_SIZE = 1;

    /// <summary>
    /// Maximum allowed page size for feed requests.
    /// Prevents memory exhaustion and slow responses.
    /// </summary>
    public const int MAX_PAGE_SIZE = 100;

    /// <summary>
    /// Maximum number of pinned posts per user.
    /// Ensures consistent UX and prevents abuse.
    /// </summary>
    public const int MAX_PINNED_POSTS = 3;

    /// <summary>
    /// Prefetch multiplier for VIP feed queries.
    /// Fetches extra items to account for filtering (suppression, blocks, mutes).
    /// </summary>
    public const int VIP_FEED_PREFETCH_MULTIPLIER = 3;

    /// <summary>
    /// Prefetch multiplier for regular feed queries.
    /// Fetches extra items to account for filtering and ranking.
    /// </summary>
    public const int REGULAR_FEED_PREFETCH_MULTIPLIER = 3;

    /// <summary>
    /// Delay between batches for fan-out operations (milliseconds).
    /// Provides backpressure to prevent overwhelming Cassandra.
    /// </summary>
    public const int FAN_OUT_BATCH_DELAY_MS = 10;

    /// <summary>
    /// Number of days to query for VIP feed.
    /// Balances data freshness and query cost.
    /// </summary>
    public const int VIP_FEED_QUERY_DAYS = 7;

    /// <summary>
    /// TTL for VIP user feed items (days).
    /// Keeps recent content while managing storage costs.
    /// </summary>
    public const int VIP_FEED_ITEM_TTL_DAYS = 30;

    /// <summary>
    /// TTL for regular user feed items (days).
    /// Shorter retention for cost optimization.
    /// </summary>
    public const int REGULAR_FEED_ITEM_TTL_DAYS = 7;

    /// <summary>
    /// Default shard count for feed partitioning.
    /// Distributes load across partitions while limiting query fan-out.
    /// </summary>
    public const int DEFAULT_SHARD_COUNT = 4;

    /// <summary>
    /// Minimum shard count for adaptive sharding.
    /// Ensures at least some parallelization.
    /// </summary>
    public const int MIN_SHARD_COUNT = 1;

    /// <summary>
    /// Maximum shard count for adaptive sharding.
    /// Limits query complexity and coordination overhead.
    /// </summary>
    public const int MAX_SHARD_COUNT = 64;

    /// <summary>
    /// Target partition size for adaptive sharding (bytes).
    /// Cassandra performs best with partitions under 100MB.
    /// </summary>
    public const long TARGET_PARTITION_SIZE_BYTES = 100_000_000; // 100 MB

    /// <summary>
    /// VIP status cache TTL (minutes).
    /// Balances accuracy of VIP classification and Redis load.
    /// </summary>
    public const int VIP_CACHE_TTL_MINUTES = 15;

    /// <summary>
    /// Feed response cache TTL (minutes).
    /// Balances freshness and cache hit ratio.
    /// </summary>
    public const int FEED_RESPONSE_CACHE_TTL_MINUTES = 30;

    /// <summary>
    /// Idempotency key cache TTL (minutes).
    /// Short-lived to handle duplicate requests during client retries.
    /// </summary>
    public const int IDEMPOTENCY_CACHE_TTL_MINUTES = 10;

    /// <summary>
    /// Ranking data cache TTL (days).
    /// Long TTL for slowly-changing ranking scores.
    /// </summary>
    public const int RANKING_CACHE_TTL_DAYS = 7;

    /// <summary>
    /// Trending posts cache TTL (hours).
    /// Updated periodically by background job.
    /// </summary>
    public const int TRENDING_CACHE_TTL_HOURS = 24;

    /// <summary>
    /// Database query timeout (milliseconds).
    /// Prevents hanging requests and provides bounded latency.
    /// </summary>
    public const int DB_QUERY_TIMEOUT_MS = 5000; // 5 seconds

    /// <summary>
    /// Database write timeout (milliseconds).
    /// Longer timeout for writes which may involve LWTs.
    /// </summary>
    public const int DB_WRITE_TIMEOUT_MS = 10000; // 10 seconds

    /// <summary>
    /// Maximum response size for idempotency cache (bytes).
    /// Prevents Redis memory exhaustion from large responses.
    /// </summary>
    public const int MAX_IDEMPOTENCY_RESPONSE_SIZE_BYTES = 256 * 1024; // 256 KB

    /// <summary>
    /// Circuit breaker failure threshold (percentage).
    /// Opens circuit after this percentage of requests fail.
    /// </summary>
    public const double CIRCUIT_BREAKER_FAILURE_THRESHOLD = 0.5; // 50%

    /// <summary>
    /// Circuit breaker minimum throughput.
    /// Circuit breaker only activates after this many requests.
    /// </summary>
    public const int CIRCUIT_BREAKER_MIN_THROUGHPUT = 10;

    /// <summary>
    /// Circuit breaker break duration (seconds).
    /// How long to keep circuit open before allowing test requests.
    /// </summary>
    public const int CIRCUIT_BREAKER_BREAK_DURATION_SECONDS = 30;

    /// <summary>
    /// Default ranking decay factor (hours).
    /// Controls how quickly post ranking decays over time.
    /// score = log(engagement) - (hours_since_creation / decay_factor)
    /// </summary>
    public const double RANKING_DECAY_FACTOR_HOURS = 24.0;

    /// <summary>
    /// Top N trending posts to fetch.
    /// Limits trending set size for performance.
    /// </summary>
    public const int TRENDING_TOP_N = 99;
}
