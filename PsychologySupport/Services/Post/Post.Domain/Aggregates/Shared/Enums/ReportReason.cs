using System.Text.Json.Serialization;

namespace Post.Domain.Aggregates.Shared.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReportReason
{
    Spam = 0,
    Abuse = 1,
    OffTopic = 2,
    Harassment = 3,
    InappropriateContent = 4,
    Misinformation = 5,
    Other = 6
}
