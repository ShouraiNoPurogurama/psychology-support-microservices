using System.Text.Json.Serialization;

namespace BuildingBlocks.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserGender
    {
        Male = 1,
        Female = 2,
        Else = 3
    }
}