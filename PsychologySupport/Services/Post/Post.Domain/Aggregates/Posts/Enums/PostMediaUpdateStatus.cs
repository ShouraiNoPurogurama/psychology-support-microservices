using System.Text.Json.Serialization;

namespace Post.Domain.Aggregates.Posts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PostMediaUpdateStatus
{
    Attached = 0,
    Removed = 1
}
