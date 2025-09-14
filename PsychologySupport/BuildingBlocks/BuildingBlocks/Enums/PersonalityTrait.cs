using System.Text.Json.Serialization;

namespace BuildingBlocks.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PersonalityTrait
    {
        None,
        Introversion,
        Extroversion,
        Adaptability,
        NotSet
    }
}