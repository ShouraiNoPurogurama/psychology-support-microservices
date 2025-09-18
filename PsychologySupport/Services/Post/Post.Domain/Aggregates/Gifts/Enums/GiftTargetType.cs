using System.Text.Json.Serialization;

namespace Post.Domain.Aggregates.Gifts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GiftTargetType
{
    Post = 0,
    Comment = 1
}
