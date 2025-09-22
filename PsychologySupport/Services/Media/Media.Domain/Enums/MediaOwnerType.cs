using System.Text.Json.Serialization;

namespace Media.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MediaOwnerType
    {
        Post,
        Comment,
        Chat,
        Profile,
        EmotionTag,
        DigitalGood,
        Audio
    }
}
