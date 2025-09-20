using Cassandra.Mapping.Attributes;

namespace Feed.Infrastructure.Persistence.Cassandra.Models;

[Table("feed_config")]
public class FeedConfigRow
{
    [PartitionKey, Column("key")]
    public string Key { get; set; } = null!;

    [Column("value")]
    public string? Value { get; set; }
}
