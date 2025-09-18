using System.Text.Json.Serialization;

namespace Post.Domain.Aggregates.Posts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PostMediaUpdateStatus
{
    Reordered = 0,
    AltTextUpdated = 1,
    Attached = 2,
    Removed = 3,
    CoverSet = 4
}
