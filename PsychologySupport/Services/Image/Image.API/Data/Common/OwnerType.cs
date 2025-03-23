using System.Text.Json.Serialization;

namespace Image.API.Data.Common
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OwnerType
    {
        Blog = 1,
        Service = 2,
        User = 3
    }
}
