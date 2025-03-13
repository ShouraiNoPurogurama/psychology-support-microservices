using System.Text.Json.Serialization;

namespace Scheduling.API.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BookingStatus
    {
        Pending,
        Paid,
        Confirmed,
        Completed,
        Cancelled
    }
}
