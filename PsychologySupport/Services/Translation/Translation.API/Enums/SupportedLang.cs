using System.Text.Json.Serialization;

namespace Translation.API.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SupportedLang
{
    en,
    vi
}
