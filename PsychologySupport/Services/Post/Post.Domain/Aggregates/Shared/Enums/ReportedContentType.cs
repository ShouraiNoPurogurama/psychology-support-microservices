using System.Text.Json.Serialization;

namespace Post.Domain.Aggregates.Shared.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReportedContentType
{
    Post = 0,
    Comment = 1
}
