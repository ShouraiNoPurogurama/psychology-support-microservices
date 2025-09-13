using System.Text.Json.Serialization;

namespace Billing.API.Data.Common
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
