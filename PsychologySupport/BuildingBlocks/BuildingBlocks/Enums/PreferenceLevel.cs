using System.Text.Json.Serialization;

namespace BuildingBlocks.Enums
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PreferenceLevel
    {
        Daily,
        Weekly,
        Occasionally,
        Like,
        Neutral,
        Dislike
    }

}