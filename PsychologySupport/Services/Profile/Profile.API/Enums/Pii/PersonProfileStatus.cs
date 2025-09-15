using System.Text.Json.Serialization;

namespace Profile.API.Enums.Pii;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PersonProfileStatus
{
    Pending,
    Active
}