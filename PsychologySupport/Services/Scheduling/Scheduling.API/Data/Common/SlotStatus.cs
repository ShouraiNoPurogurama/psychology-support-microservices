using System.Text.Json.Serialization;

namespace Scheduling.API.Data.Common
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SlotStatus
    {
            Available,
            Unavailable
    }
}
