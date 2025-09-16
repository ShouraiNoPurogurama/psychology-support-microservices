using System.Text.Json.Serialization;

namespace Post.Domain.Aggregates.Reactions.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReactionTargetType
{
    Post = 0,
    Comment = 1
}
