using System.Text.Json.Serialization;

namespace Billing.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum InvoiceStatus
    {
        Issued,
        Paid,
        Void,
        Cancelled
    }
}
