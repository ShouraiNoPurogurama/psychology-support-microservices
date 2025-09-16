using System.Text.Json.Serialization;

namespace Post.Domain.Aggregates.Comments.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CommentModerationStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Flagged = 3
}
