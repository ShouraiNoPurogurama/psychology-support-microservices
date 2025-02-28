using System.Text.Json.Serialization;

namespace BuildingBlocks.Data.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserGender
    {
        Male = 1,
        Female = 2,
        Else = 3
    }
}