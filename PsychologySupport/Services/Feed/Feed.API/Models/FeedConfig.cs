using Cassandra.Mapping.Attributes;

namespace Feed.API.Models
{
    // Quy ước thời gian:
    // - LocalDate: trường *_ymd, *_ymd_bucket (ngày-tháng-năm, không giờ)
    // - DateTimeOffset: trường *_at, *_since, *_until, *_created_at (UTC)
    // - TimeUuid: trường mang tính "event-time + uniqueness" (seen_at, pinned_at, ts_uuid)

    [Table("feed_config")]
    public class FeedConfig
    {
        [PartitionKey, Column("key")]
        public string Key { get; set; } = null!;

        [Column("value")]
        public string? Value { get; set; }
    }
}
