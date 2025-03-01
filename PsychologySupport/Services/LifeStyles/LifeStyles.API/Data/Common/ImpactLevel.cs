using System.Text.Json.Serialization;

namespace LifeStyles.API.Data.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ImpactLevel
{
    Low,
    Medium,
    High,
    VeryHigh
}