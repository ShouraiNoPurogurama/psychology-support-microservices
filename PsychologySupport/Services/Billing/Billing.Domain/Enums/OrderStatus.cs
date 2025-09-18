using System.Text.Json.Serialization;

namespace Billing.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderStatus
    {
        Pending,
        Paid,
        Failed,
        Cancelled
    }
}
