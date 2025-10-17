using System.Text.Json.Serialization;

namespace Alias.API.Aliases.Models.Aliases.Enums;
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PreferenceTheme
{
    Light = 1,
    Dark = 2
}
