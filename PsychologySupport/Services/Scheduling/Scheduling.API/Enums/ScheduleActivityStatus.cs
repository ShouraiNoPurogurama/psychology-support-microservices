using System.Text.Json.Serialization;

namespace Scheduling.API.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ScheduleActivityStatus
    {
        Pending,
        Completed,
        Cancelled
    }
}
