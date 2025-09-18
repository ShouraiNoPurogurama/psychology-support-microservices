using System.Text.Json.Serialization;

namespace Post.Domain.Aggregates.CategoryTags.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CategoryTagUpdateStatus
{
    Attached = 0,
    Detached = 1,
    Updated = 2
}
