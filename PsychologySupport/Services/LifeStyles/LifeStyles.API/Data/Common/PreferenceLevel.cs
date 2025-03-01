using System.Text.Json.Serialization;

namespace LifeStyles.API.Data.Common;

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