using System.Text.Json.Serialization;

namespace Test.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SeverityLevel
{
    Normal,
    Mild,
    Moderate,
    Severe,
    Extreme
}