using System.Text.Json.Serialization;

namespace Post.Domain.Aggregates.Posts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LifecycleStatus
{
    Draft = 0,
    Published = 1,
    Unpublished = 2,
    Archived = 3,
    Deleted = 4,
    Restored = 5
}
