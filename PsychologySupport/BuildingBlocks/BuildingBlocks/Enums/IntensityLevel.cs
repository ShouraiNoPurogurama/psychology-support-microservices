using System.Text.Json.Serialization;

namespace BuildingBlocks.Enums
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum IntensityLevel
    {
        Low,
        Medium,
        High,
        VeryHigh
    }
}