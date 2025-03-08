using System.Text.Json.Serialization;

namespace BuildingBlocks.Enums
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ImpactLevel
    {
        Low,
        Medium,
        High,
        VeryHigh
    }
}