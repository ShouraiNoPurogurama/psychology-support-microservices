using System.Text.Json.Serialization;

namespace Auth.API;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DeviceType
{
    Android = 1,
    IOS = 2,
    Web = 3
}