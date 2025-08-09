using System.Text.Json.Serialization;

namespace BuildingBlocks.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SupportedLang
{
    en,
    vi
}
