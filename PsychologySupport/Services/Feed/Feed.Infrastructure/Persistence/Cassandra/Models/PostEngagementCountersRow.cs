using Cassandra.Mapping.Attributes;

namespace Feed.Infrastructure.Persistence.Cassandra.Models;

/// <summary>
/// Cassandra row model for PostEngagementCounters table.
/// Uses counter columns for tracking various engagement metrics.
/// </summary>
[Table("PostEngagementCounters")]
public class PostEngagementCountersRow
{
    [PartitionKey, Column("post_id")]
    public Guid PostId { get; set; }

    [Counter, Column("reactions")]
    public long? Reactions { get; set; }

    [Counter, Column("comments")]
    public long? Comments { get; set; }

    [Counter, Column("shares")]
    public long? Shares { get; set; }

    [Counter, Column("clicks")]
    public long? Clicks { get; set; }

    [Counter, Column("impressions")]
    public long? Impressions { get; set; }

    [Counter, Column("view_duration_sec")]
    public long? ViewDurationSec { get; set; }
}
