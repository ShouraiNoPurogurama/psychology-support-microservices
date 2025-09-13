using System.Text.Json.Serialization;

namespace Billing.API.Data.Common
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
