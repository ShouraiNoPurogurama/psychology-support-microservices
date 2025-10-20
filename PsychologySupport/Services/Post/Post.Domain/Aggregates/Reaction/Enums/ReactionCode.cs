using System.Text.Json.Serialization;

namespace Post.Domain.Aggregates.Reaction.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReactionCode
{
    Like = 0,
    Heart = 1,
    Laugh = 2,
    Angry = 3,
    Sad = 4,
    Wow = 5
}
