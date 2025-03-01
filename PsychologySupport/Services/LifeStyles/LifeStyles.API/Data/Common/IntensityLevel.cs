using System.Text.Json.Serialization;

namespace LifeStyles.API.Data.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IntensityLevel
{
    Low,
    Medium,
    High,
    VeryHigh
}